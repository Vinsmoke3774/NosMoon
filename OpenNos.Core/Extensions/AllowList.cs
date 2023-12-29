using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace OpenNos.Core.Extensions
{
    public class AllowList
    {
        IList<string> _allowList;

        public AllowList()
        {
            _allowList = new List<string>();
        }

        public ReadOnlyCollection<string> ToList
        {
            get
            {
                return new(_allowList);
            }
        }

        public void Add(string wordToAllowlist)
        {
            if (string.IsNullOrEmpty(wordToAllowlist)) return;

            if (!_allowList.Contains(wordToAllowlist.ToLower(CultureInfo.InvariantCulture))) _allowList.Add(wordToAllowlist.ToLower(CultureInfo.InvariantCulture));
        }

        public bool Contains(string wordToCheck)
        {
            if (string.IsNullOrEmpty(wordToCheck)) return false;

            return _allowList.Contains(wordToCheck.ToLower(CultureInfo.InvariantCulture));
        }

        public int Count
        {
            get
            {
                return _allowList.Count;
            }
        }

        public void Clear()
        {
            _allowList.Clear();
        }

        public bool Remove(string wordToRemove)
        {
            if (string.IsNullOrEmpty(wordToRemove)) return false;

            return _allowList.Remove(wordToRemove.ToLower(CultureInfo.InvariantCulture));
        }
    }
}
