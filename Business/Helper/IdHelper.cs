using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Business.Helper
{
    public static class IdHelper
    {
       
        public static async Task<bool> IsIdExist(IRepository<Product> repository, int id)
        {
            IEnumerable<int> ids = (await repository.GetAllAsync()).Select(x => x.Id);

            return IsIdExistFromIds(ids, id);
        }
        public static async Task<BaseEntity> MakeId(IRepository<ReceiptDetail> repository, BaseEntity entity)
        {
            IEnumerable<int> ids = (await repository.GetAllAsync()).Select(x => x.Id);

            return MakeIdFromIds(ids, entity);
        }

        private static BaseEntity MakeIdFromIds(IEnumerable<int> ids, BaseEntity entity)
        {
            if (entity.Id == 0)
            {
                int newId = 1;

                bool flag = true;

                while (flag)
                {
                    flag = false;
                    foreach (int id in ids)
                    {
                        if (id == newId)
                        {
                            flag = true;
                            newId++;
                            break;
                        }

                    }
                }

                entity.Id = newId;
            }
            return entity;
        }

        private static bool IsIdExistFromIds(IEnumerable<int> ids, int Checkedid)
        {
            foreach (int id in ids)
            {
                if(Checkedid == id)
                    return true;
            }
            return false;
        }
    }
}
