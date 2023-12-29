namespace OpenNos.Core
{
    public class Singleton<T> where T : class, new()
    {
        #region Members

        private static T _instance;

        #endregion

        #region Properties

        public static T Instance => _instance ??= new T();

        #endregion
    }
}