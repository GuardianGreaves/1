using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class EventsManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter adapter;

        public EventsManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Название == name);
            if (row != null)
            {
                return row.ID_Мероприятия;
            }
            return -1;
        }

        public bool SetUserRoleByName(DataRow row, string name)
        {
            int id = GetIdByName(name);
            if (id == -1)
            {
                return false;
            }
            row["ID_Мероприятия"] = id;
            return true;
        }
    }
}