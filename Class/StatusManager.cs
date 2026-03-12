using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class StatusManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter adapter;

        public StatusManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Название == name);
            if (row != null)
            {
                return row.ID_Статуса;
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
            row["ID_Статуса"] = id;
            return true;
        }
    }
}
