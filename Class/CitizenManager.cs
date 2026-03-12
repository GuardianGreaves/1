using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class CitizenManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter adapter;

        public CitizenManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Фамилия + ' ' + r.Имя + ' ' + r.Отчество == name);
            if (row != null)
            {
                return row.ID_Гражданина;
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
            row["ID_Гражданина"] = id;
            return true;
        }
    }
}