using System.Data;
using System.Linq;

namespace diplom_loskutova.Class
{
    public class UserManager
    {
        private DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter adapter;

        public UserManager()
        {
            adapter = new DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter();
        }

        public int GetIdByName(string name)
        {
            var table = adapter.GetData();
            var row = table.FirstOrDefault(r => r.Фамилия + ' ' + r.Имя + ' ' + r.Отчество == name);
            if (row != null)
            {
                return row.ID_Пользователя;
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
            row["ID_Пользователя"] = id;
            return true;
        }
    }
}
