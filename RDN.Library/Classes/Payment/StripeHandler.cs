﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDN.Library.Classes.Error;
using RDN.Library.Classes.Payment.Classes.Invoice;
using RDN.Library.Classes.Payment.Enums;
using RDN.Library.Classes.Payment.Enums.Stripe;
using RDN.Library.DataModels.Context;
using RDN.Library.DataModels.PaymentGateway.Stripe;
using RDN.Utilities.Config;
using RDN.Utilities.Dates;
using Stripe;
using RDN.Portable.Config;
using RDN.Portable.Classes.Payment.Enums;

namespace RDN.Library.Classes.Payment
{
    public class StripeHandler
    {
        /// <summary>
        /// no need to do anything here just yet.
        /// I think when leagues start changing their subscriptions, we will need to update this code to change the plan
        /// 
        /// </summary>
        /// <param name="se"></param>
        /// <returns></returns>
        public static bool SubscriptionUpdated(StripeEvent se, string json)
        {
            try
            {
                //{ "id": "evt_1Myn3gX10D21o6", "created": 1361989292, "livemode": true, "type": "customer.subscription.updated", "data": { "object": { "plan": { "interval": "month", "name": "Monthly_Plan", "amount": 1599, "currency": "usd", "id": "Monthly_Plan", "object": "plan", "livemode": true, "interval_count": 1, "trial_period_days": null }, "object": "subscription", "start": 1359310832, "status": "active", "customer": "cus_1BMmWWvjng8Mug", "cancel_at_period_end": false, "current_period_start": 1361989232, "current_period_end": 1364408432, "ended_at": null, "trial_start": null, "trial_end": null, "canceled_at": null, "quantity": 1 }, "previous_attributes": { "current_period_start": 1359310832, "current_period_end": 1361989232 } }, "object": "event", "pending_webhooks": 1 } 

                PaymentGateway pg = new PaymentGateway();
                var f = pg.StartInvoiceWizard().Initalize(ServerConfig.RDNATION_STORE_ID, "USD",
PaymentProvider.Stripe, PaymentMode.Live, ChargeTypeEnum.SubscriptionUpdated)
                    .SetInvoiceId(Guid.NewGuid())
                    .SetInvoiceStatus(InvoiceStatus.Subscription_Should_Be_Updated_On_Charge);


                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    StripeSubscription cust = Stripe.Mapper<StripeSubscription>.MapFromJson(se.Data.Object.ToString());
                    //StripeSubscription cust = (StripeSubscription)se.Data.Object;
                    StripeSubscriptionDb custDb = new StripeSubscriptionDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.customer_subscription_created;
                    custDb.CanceledAt = cust.CanceledAt;
                    custDb.EndedAt = cust.EndedAt;
                    custDb.Customer = dc.StripeCustomers.Where(x => x.Id == cust.CustomerId).FirstOrDefault();
                    custDb.PeriodEnd = cust.PeriodEnd;
                    custDb.PeriodStart = cust.PeriodStart;
                    custDb.Quantity = cust.Quantity;
                    custDb.Start = cust.Start;
                    custDb.Status = cust.Status;
                    if (cust.StripePlan != null)
                    {
                        custDb.StripePlan = dc.StripePlans.Where(x => x.Id == cust.StripePlan.Id).FirstOrDefault();
                        if (custDb.StripePlan == null)
                        {
                            StripePlanDb pl = new StripePlanDb();
                            pl.AmountInCents = cust.StripePlan.AmountInCents;
                            pl.Currency = cust.StripePlan.Currency;
                            pl.Id = cust.StripePlan.Id;
                            pl.Interval = cust.StripePlan.Interval;
                            pl.IntervalCount = cust.StripePlan.IntervalCount;
                            pl.LiveMode = cust.StripePlan.LiveMode;
                            pl.Name = cust.StripePlan.Name;
                            pl.TrialPeriodDays = cust.StripePlan.TrialPeriodDays;
                            custDb.StripePlan = pl;
                            dc.StripePlans.Add(pl);
                        }
                    }
                    DateTime now = DateTime.UtcNow;
                    TimeSpan ts = new TimeSpan();
                    int lengthOfDays = 31;

                    SubscriptionPeriodStripe period = SubscriptionPeriodStripe.Monthly;
                    if (cust.StripePlan.Id == StripePlanNames.Monthly_Plan.ToString())
                    {
                        period = SubscriptionPeriodStripe.Monthly;
                        ts = now.AddMonths(1) - DateTime.UtcNow;
                        lengthOfDays = ts.Days;
                    }
                    else if (cust.StripePlan.Id == StripePlanNames.Six_Month_League_Subscription.ToString())
                    {
                        period = SubscriptionPeriodStripe.Six_Months;
                        ts = now.AddMonths(6) - DateTime.UtcNow;
                        lengthOfDays = ts.Days;
                    }
                    else if (cust.StripePlan.Id == StripePlanNames.Three_Month_League_Subscription.ToString())
                    {
                        period = SubscriptionPeriodStripe.Three_Months;
                        ts = now.AddMonths(3) - DateTime.UtcNow;
                        lengthOfDays = ts.Days;
                    }
                    else if (cust.StripePlan.Id == StripePlanNames.Yearly_League_Subscription.ToString())
                    {
                        period = SubscriptionPeriodStripe.Yearly;
                        ts = now.AddMonths(12) - DateTime.UtcNow;
                        lengthOfDays = ts.Days;
                    }
                    //getting league Id here and subscriptionDate
                    var invoiceFromPast = pg.GetDisplayInvoiceWithStripeCustomerId(cust.CustomerId);
                    Guid leagueId = new Guid();
                    if (invoiceFromPast.Subscription != null)
                    {
                        leagueId = invoiceFromPast.Subscription.InternalObject;
                    }
                    f.SetPaymentProviderId(cust.CustomerId);
                    f.SetSubscription(new InvoiceSubscription
                    {
                        Description = "RDN League portal subscription",
                        DescriptionRecurring = "Fee for RDN League portal subscription",
                        Name = "RDN Member portal",
                        NameRecurring = "RDN Member portal recurring",
                        DigitalPurchaseText = "You have now access to RDN League portal",
                        Price = Convert.ToDecimal(cust.StripePlan.AmountInCents) / Convert.ToDecimal(100),
                        SubscriptionPeriodStripe = period,
                        SubscriptionPeriodLengthInDays = lengthOfDays,
                        //league id is the ownerId
                        InternalObject = leagueId
                    });

                    custDb.TrialEnd = cust.TrialEnd;
                    custDb.TrialStart = cust.TrialStart;

                    dc.StripeSubscriptions.Add(custDb);
                    even.Subscription = custDb;
                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();

                f.FinalizeInvoice();

                EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: New Subscription About to be charged!!", json);
                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }

        public static bool SubscriptionCreated(StripeEvent se, string json)
        {
            try
            {
                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    StripeSubscription cust = Stripe.Mapper<StripeSubscription>.MapFromJson(se.Data.Object.ToString());
                    //StripeSubscription cust = (StripeSubscription)se.Data.Object;
                    StripeSubscriptionDb custDb = new StripeSubscriptionDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.customer_subscription_created;
                    custDb.CanceledAt = cust.CanceledAt;
                    custDb.EndedAt = cust.EndedAt;
                    custDb.Customer = dc.StripeCustomers.Where(x => x.Id == cust.CustomerId).FirstOrDefault();
                    custDb.PeriodEnd = cust.PeriodEnd;
                    custDb.PeriodStart = cust.PeriodStart;
                    custDb.Quantity = cust.Quantity;
                    custDb.Start = cust.Start;
                    custDb.Status = cust.Status;
                    if (cust.StripePlan != null)
                    {
                        custDb.StripePlan = dc.StripePlans.Where(x => x.Id == cust.StripePlan.Id).FirstOrDefault();
                        if (custDb.StripePlan == null)
                        {
                            StripePlanDb pl = new StripePlanDb();
                            pl.AmountInCents = cust.StripePlan.AmountInCents;
                            pl.Currency = cust.StripePlan.Currency;
                            pl.Id = cust.StripePlan.Id;
                            pl.Interval = cust.StripePlan.Interval;
                            pl.IntervalCount = cust.StripePlan.IntervalCount;
                            pl.LiveMode = cust.StripePlan.LiveMode;
                            pl.Name = cust.StripePlan.Name;
                            pl.TrialPeriodDays = cust.StripePlan.TrialPeriodDays;
                            custDb.StripePlan = pl;
                            dc.StripePlans.Add(pl);
                        }
                    }
                    custDb.TrialEnd = cust.TrialEnd;
                    custDb.TrialStart = cust.TrialStart;

                    dc.StripeSubscriptions.Add(custDb);
                    even.Subscription = custDb;
                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }


        public static bool InvoicePaymentSucceeded(StripeEvent se, string json)
        {
            try
            {
                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    //StripeInvoice inv = (StripeInvoice)se.Data.Object;
                    StripeInvoice inv = Stripe.Mapper<StripeInvoice>.MapFromJson(se.Data.Object.ToString());
                    StripeInvoiceDb nnv = new StripeInvoiceDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.invoice_created;
                    nnv.AmountDueInCents = inv.AmountDueInCents;
                    nnv.AttemptCount = inv.AttemptCount;
                    nnv.Attempted = inv.Attempted;
                    nnv.ChargeId = inv.ChargeId;
                    nnv.Closed = inv.Closed;
                    nnv.Customer = dc.StripeCustomers.Where(x => x.Id == inv.CustomerId).FirstOrDefault();
                    nnv.Date = inv.Date;
                    nnv.EndingBalanceInCents = inv.EndingBalanceInCents;
                    nnv.Id = inv.Id;
                    nnv.LiveMode = inv.LiveMode;
                    nnv.NextPaymentAttempt = inv.NextPaymentAttempt;
                    nnv.Object = inv.Object;
                    nnv.Paid = inv.Paid;
                    nnv.PeriodEnd = inv.PeriodEnd;
                    nnv.PeriodStart = inv.PeriodStart;
                    nnv.StartingBalanceInCents = inv.StartingBalanceInCents;
                    nnv.SubtotalInCents = inv.SubtotalInCents;
                    nnv.TotalInCents = inv.TotalInCents;
                    even.Invoice = nnv;
                    //commented out because the version upgraded to 2.7 from 2.2
                    //if (inv.StripeInvoiceLines != null && inv.StripeInvoiceLines.StripeInvoiceSubscriptions != null)
                    //{
                    //    foreach (var sub in inv.StripeInvoiceLines.StripeInvoiceSubscriptions)
                    //    {
                    //        nnv.SubscriptionPlan = dc.StripePlans.Where(x => x.Id == sub.StripePlan.Id).FirstOrDefault();
                    //    }
                    //}

                    dc.StripeInvoices.Add(nnv);
                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }

        public static void EmailLeagueAboutCardDeclinedSubscription(Guid leagueId, Guid invoiceId, string reasonForDecline, string updateUrl, string secondEmail)
        {
            try
            {
                var league = League.LeagueFactory.GetLeague(leagueId);
                var emailData = new Dictionary<string, string>
                                        {
                                            { "leaguename",league.Name}, 
                                            { "invoiceId", invoiceId.ToString().Replace("-","") },
                                            { "reasonForDecline", reasonForDecline},
                                            { "tryAgainUrl", updateUrl}
                                        };
                EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, league.Email, EmailServer.EmailServer.DEFAULT_SUBJECT + " Card Was Declined For League Subscription", emailData, EmailServer.EmailServerLayoutsEnum.SubscriptionCardWasDeclined);
                if (league.Email != secondEmail && !String.IsNullOrEmpty(secondEmail))
                {
                    EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, secondEmail, EmailServer.EmailServer.DEFAULT_SUBJECT + " Card Was Declined For League Subscription", emailData, EmailServer.EmailServerLayoutsEnum.ReceiptForLeagueSubscription);
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
        }
        public static bool ChargeFailed(StripeEvent se, string json)
        {
            try
            {

                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    //StripeCharge inv = (StripeCharge)se.Data.Object;
                    StripeCharge inv = Stripe.Mapper<StripeCharge>.MapFromJson(se.Data.Object.ToString());
                    StripeChargeDb nnv = new StripeChargeDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.charge_failed;
                    nnv.AmountInCents = inv.AmountInCents;
                    nnv.AmountInCentsRefunded = inv.AmountInCentsRefunded;
                    nnv.Currency = inv.Currency;
                    nnv.Customer = dc.StripeCustomers.Where(x => x.Id == inv.CustomerId).FirstOrDefault();
                    nnv.Description = inv.Description;
                    nnv.FailureMessage = inv.FailureMessage;

                    nnv.FeeInCents = inv.FeeInCents;
                    nnv.Id = inv.Id;
                    nnv.Invoice = dc.StripeInvoices.Where(x => x.Id == inv.InvoiceId).FirstOrDefault();
                    nnv.LiveMode = inv.LiveMode;
                    nnv.Paid = inv.Paid;
                    nnv.Refunded = inv.Refunded;
                    if (inv.StripeCard != null)
                    {
                        nnv.StripeCard = dc.StripeCards.Where(x => x.AddressLine1 == inv.StripeCard.AddressLine1).Where(x => x.Last4 == inv.StripeCard.Last4).FirstOrDefault();
                        if (nnv.StripeCard == null)
                        {
                            nnv.StripeCard = CreateStripeCard(inv.StripeCard, json);
                            dc.StripeCards.Add(nnv.StripeCard);
                        }
                    }
                    even.Charge = nnv;
                    dc.StripeCharges.Add(nnv);

                    var invoice = (from xx in dc.Invoices
                                   where xx.PaymentProviderCustomerId == inv.CustomerId
                                   where xx.InvoicePaid == false
                                   select xx).OrderByDescending(x => x.Created).FirstOrDefault();
                    if (invoice == null)
                        EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: Invoice Not Found, Can't Be Confirmed, CHARGE FAILED", inv.CustomerId + " " + inv.ToString() + json);
                    else
                    {
                        if (invoice.InvoiceStatus == (byte)InvoiceStatus.Subscription_Should_Be_Updated_On_Charge)
                        {
                            //update league subscription
                            //League.League.UpdateLeagueSubscriptionPeriod(invoice.Subscription.ValidUntil, true, invoice.Subscription.InternalObject);
                            //EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: Subscription Updated!!", invoice.InvoiceId + " Amount:" + inv.AmountInCents + ":" + inv.ToString() + json);
                        }

                        invoice.InvoicePaid = false;
                        if (nnv.FailureMessage == null)
                        {
                            invoice.InvoiceStatus = (byte)InvoiceStatus.Failed;
                            nnv.FailureMessage = "Payment Declined, Please contact RDNation @ info@rdnation.com.";
                        }
                        else if (nnv.FailureMessage.Contains("Your card number is incorrect"))
                            invoice.InvoiceStatus = (byte)InvoiceStatus.Card_Was_Declined;
                        else
                            invoice.InvoiceStatus = (byte)InvoiceStatus.Failed;

                        invoice.InvoiceStatusUpdated = DateTime.UtcNow;
                        invoice.Merchant = invoice.Merchant;

                        var customerService = new StripeCustomerService();
                        StripeSubscription subscription = customerService.CancelSubscription(inv.CustomerId, true);

                        EmailLeagueAboutCardDeclinedSubscription(invoice.Subscription.InternalObject, invoice.InvoiceId, nnv.FailureMessage, ServerConfig.LEAGUE_SUBSCRIPTION_UPDATESUBSUBSCRIBE + invoice.Subscription.InternalObject.ToString().Replace("-", ""), ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN);
                    }

                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();

                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }

        public static bool ChargeSucceeded(StripeEvent se, string json)
        {
            try
            {

                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    //StripeCharge inv = (StripeCharge)se.Data.Object;
                    StripeCharge inv = Stripe.Mapper<StripeCharge>.MapFromJson(se.Data.Object.ToString());
                    StripeChargeDb nnv = new StripeChargeDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.charge_succeeded;
                    nnv.AmountInCents = inv.AmountInCents;
                    nnv.AmountInCentsRefunded = inv.AmountInCentsRefunded;
                    nnv.Currency = inv.Currency;
                    nnv.Customer = dc.StripeCustomers.Where(x => x.Id == inv.CustomerId).FirstOrDefault();
                    nnv.Description = inv.Description;
                    nnv.FailureMessage = inv.FailureMessage;
                    foreach (var fee in inv.FeeDetails)
                    {
                        StripeFeeDb f = new StripeFeeDb();
                        f.AmountInCents = fee.AmountInCents;
                        f.Application = fee.Application;
                        f.Currency = fee.Currency;
                        f.Description = fee.Description;
                        f.type = fee.type;
                        nnv.FeeDetails.Add(f);
                    }

                    nnv.FeeInCents = inv.FeeInCents;
                    nnv.Id = inv.Id;
                    nnv.Invoice = dc.StripeInvoices.Where(x => x.Id == inv.InvoiceId).FirstOrDefault();
                    nnv.LiveMode = inv.LiveMode;
                    nnv.Paid = inv.Paid;
                    nnv.Refunded = inv.Refunded;
                    if (inv.StripeCard != null)
                    {
                        nnv.StripeCard = dc.StripeCards.Where(x => x.AddressLine1 == inv.StripeCard.AddressLine1).Where(x => x.Last4 == inv.StripeCard.Last4).FirstOrDefault();
                        if (nnv.StripeCard == null)
                        {
                            nnv.StripeCard = CreateStripeCard(inv.StripeCard, json);
                            dc.StripeCards.Add(nnv.StripeCard);
                        }
                    }
                    even.Charge = nnv;
                    dc.StripeCharges.Add(nnv);

                    var invoice = (from xx in dc.Invoices
                                   where xx.PaymentProviderCustomerId == inv.CustomerId
                                   where xx.InvoicePaid == false
                                   select xx).OrderByDescending(x => x.Created).FirstOrDefault();
                    if (invoice == null)
                        EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: Invoice Not Found, Can't Be Confirmed", inv.CustomerId + " " + inv.ToString() + json);
                    else
                    {
                        if (invoice.InvoiceStatus == (byte)InvoiceStatus.Subscription_Should_Be_Updated_On_Charge)
                        {
                            //update league subscription
                            League.LeagueFactory.UpdateLeagueSubscriptionPeriod(invoice.Subscription.ValidUntil, true, invoice.Subscription.InternalObject);
                            EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: Subscription Updated!!", invoice.InvoiceId + " Amount:" + inv.AmountInCents + ":" + inv.ToString() + json);
                        }

                        invoice.InvoicePaid = true;
                        invoice.InvoiceStatus = (byte)InvoiceStatus.Payment_Successful;
                        invoice.InvoiceStatusUpdated = DateTime.UtcNow;
                        invoice.Merchant = invoice.Merchant;
                        EmailServer.EmailServer.SendEmail(ServerConfig.DEFAULT_EMAIL, ServerConfig.DEFAULT_EMAIL_FROM_NAME, ServerConfig.DEFAULT_ADMIN_EMAIL_ADMIN, "STRIPE: New Payment Made!!", invoice.InvoiceId + " Amount:" + inv.AmountInCents + ":" + inv.ToString() + json);
                    }

                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();

                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }


        public static bool InvoiceCreated(StripeEvent se, string json)
        {
            try
            {
                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    StripeInvoice inv = Stripe.Mapper<StripeInvoice>.MapFromJson(se.Data.Object.ToString());
                    //StripeInvoice inv = (StripeInvoice)se.Data.Object;
                    StripeInvoiceDb nnv = new StripeInvoiceDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.invoice_created;
                    nnv.AmountDueInCents = inv.AmountDueInCents;
                    nnv.AttemptCount = inv.AttemptCount;
                    nnv.Attempted = inv.Attempted;
                    //nnv.ChargeId = dc.StripeCharges.Where(x => x.Id == inv.ChargeId).FirstOrDefault();
                    nnv.ChargeId = inv.ChargeId;
                    nnv.Closed = inv.Closed;

                    nnv.Customer = dc.StripeCustomers.Where(x => x.Id == inv.CustomerId).FirstOrDefault();
                    nnv.Date = inv.Date;
                    nnv.EndingBalanceInCents = inv.EndingBalanceInCents;
                    nnv.Id = inv.Id;
                    nnv.LiveMode = inv.LiveMode;
                    nnv.NextPaymentAttempt = inv.NextPaymentAttempt;
                    nnv.Object = inv.Object;
                    nnv.Paid = inv.Paid;
                    nnv.PeriodEnd = inv.PeriodEnd;
                    nnv.PeriodStart = inv.PeriodStart;
                    nnv.StartingBalanceInCents = inv.StartingBalanceInCents;
                    nnv.SubtotalInCents = inv.SubtotalInCents;
                    nnv.TotalInCents = inv.TotalInCents;
                    even.Invoice = dc.StripeInvoices.Where(x => x.Id == nnv.Id).FirstOrDefault();
                    dc.StripeInvoices.Add(nnv);
                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }


        public static bool CustomerCreated(StripeEvent se, string json)
        {
            try
            {
                var dc = new ManagementContext();
                RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb even = new RDN.Library.DataModels.PaymentGateway.Stripe.StripeEventDb();
                even.CreatedStripeDate = se.Created.GetValueOrDefault();
                even.StripeId = se.Id;
                even.LiveMode = se.LiveMode.GetValueOrDefault();
                if (se.Data != null)
                {
                    //StripeCustomer cust = (StripeCustomer)se.Data.Object;
                    StripeCustomer cust = Stripe.Mapper<StripeCustomer>.MapFromJson(se.Data.Object.ToString());
                    StripeCustomerDb custDb = new StripeCustomerDb();
                    even.StripeEventTypeEnum = (byte)StripeEventTypeEnum.customer_created;
                    custDb.CreatedByStripe = cust.Created;
                    custDb.Email = cust.Email;
                    custDb.Deleted = cust.Deleted;
                    custDb.Description = cust.Description;
                    custDb.Id = cust.Id;
                    even.Customer = dc.StripeCustomers.Where(x => x.Id == cust.Id).FirstOrDefault();
                    if (cust.StripeCardList != null)
                    {
                        foreach (var card in cust.StripeCardList.StripeCards)
                        {
                            custDb.StripeCard = CreateStripeCard(card, json);
                            dc.StripeCards.Add(custDb.StripeCard);
                        }
                    }
                    dc.StripeCustomers.Add(custDb);
                }
                dc.StripeEvents.Add(even);
                dc.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return false;
        }

        private static StripeCardDb CreateStripeCard(StripeCard car, string json)
        {
            StripeCardDb card = new StripeCardDb();
            try
            {
                card.AddressCity = car.AddressCity;
                card.AddressCountry = car.AddressCountry;
                card.AddressLine1 = car.AddressLine1;
                card.AddressLine1Check = car.AddressLine1Check;
                card.AddressLine2 = car.AddressLine2;
                card.AddressState = car.AddressState;
                card.AddressZip = car.AddressZip;
                card.AddressZipCheck = car.AddressZipCheck;
                card.Country = car.Country;
                card.CvcCheck = car.CvcCheck;
                card.ExpirationMonth = car.ExpirationMonth;
                card.ExpirationYear = car.ExpirationYear;
                card.Fingerprint = car.Fingerprint;
                card.Last4 = car.Last4;
                card.Type = car.Type;
                card.Name = car.Name;
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType(), additionalInformation: json);
            }
            return card;
        }

    }
}
