﻿using System.Collections.Generic;

namespace AdminApps.DAL.Services
{
    public interface IRepository<T> where T : class
    {
        IList<T> GetAll();
        T GetById(int id);
        void Update(T entity);
        void Insert(T entity);
        void Delete(int id);
    }
}
