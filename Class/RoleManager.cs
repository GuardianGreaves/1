using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class RoleManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.РОЛЬTableAdapter adapter;

        public RoleManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.РОЛЬTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Название == name);
            if (row != null)
            {
                return row.ID_Роли;
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
            row["ID_Роли"] = id;
            return true;
        }

        public string GetNameById(int id)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.ID_Роли == id);
            if (row != null)
            {
                return row.Название;
            }
            return null; // или пустая строка ""
        }
    }
}