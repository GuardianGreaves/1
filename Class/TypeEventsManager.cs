using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class TypeEventsManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter adapter;

        public TypeEventsManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Название == name);
            if (row != null)
            {
                return row.ID_Типа;
            }
            return -1;
        }

        public bool SetByName(DataRow row, string name)
        {
            int id = GetIdByName(name);
            if (id == -1)
            {
                return false;
            }
            row["ID_Типа"] = id;
            return true;
        }
    }
}
