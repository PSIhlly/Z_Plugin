using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Z_DesignStyle
{
    
    public class RawData
        {
            public IDatabase database;
            public int id;
        }
    public interface IDatabase
    {
       
        public bool Load(bool loadSubDatabase);
        public void Save(bool saveSubDatabase);
        public void Init();
        public void Register(RawData upData, string address, string fileName);
    }
    public abstract class  Z_Database: IDatabase
    {
        // Start is called before the first frame update
        public string address;
        public string fileName;
        public RawData upData;
        
        public virtual RawData GetRawData(int id)
        {
            return new RawData();
        }

        public virtual bool Load(bool loadSubDatabase)
        {
            if(File.Exists(address+fileName))
            {
                return true;
            }
            return false;
        }

        public virtual void Save(bool saveSubDatabase)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Init()
        {
            throw new System.NotImplementedException();
        }
        public virtual void Register(RawData upData,string address,string fileName)
        {
            this.upData = (RawData)upData;
            this.address = address;
            this.fileName = fileName;
            if(!Directory.Exists(address))
            {
                Directory.CreateDirectory(address);
            }
            if (!File.Exists(address+ fileName))
            {
                File.Create(address+ fileName).Close();
            }
        }
    }
}