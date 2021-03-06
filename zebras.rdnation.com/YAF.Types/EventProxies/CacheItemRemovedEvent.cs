/* Yet Another Forum.NET
 * Copyright (C) 2006-2012 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
namespace YAF.Types.EventProxies
{
  #region Using

  using System.Web.Caching;

  using YAF.Types.Interfaces;

  #endregion

  /// <summary>
  /// The cache item removed event.
  /// </summary>
  public class CacheItemRemovedEvent : IAmEvent
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItemRemovedEvent"/> class.
    /// </summary>
    /// <param name="key">
    /// The key.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    /// <param name="reason">
    /// The reason.
    /// </param>
    public CacheItemRemovedEvent([NotNull] string key, [NotNull] object value, CacheItemRemovedReason reason)
    {
      this.Key = key;
      this.Value = value;
      this.Reason = reason;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets Key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets Reason.
    /// </summary>
    public CacheItemRemovedReason Reason { get; set; }

    /// <summary>
    /// Gets or sets Value.
    /// </summary>
    public object Value { get; set; }

    #endregion
  }
}