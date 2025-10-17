using AdminBaseComponenets;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Models;

namespace AdminClient.Pages
{
    public partial class Page 
    {

       

        interface CCC0
        {
            IEnumerable<T> getData<T>();
            object getData0();
            int getDataSize();
        }
        class CCC<TItem, TKEY> : CCC0
         where TItem : class, Models.IIdMapper<TKEY>
                where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {



            public IEnumerable<T> getData<T>()
            {
                return value as IEnumerable<T>;
            }
            public object getData0()
            {
                return value;
            }
            public int getDataSize()
            {
                return value.Count();
            }




            public IEnumerable<TItem> value { get; set; } = null;
            public async Task f(bool forceLoad=false)
            {
                Console.WriteLine($"call f {typeof(TItem).Name} {typeof(TKEY).Name}");
                value = await Program0.getEntityManager<TItem, TKEY>().getAll(forceLoad);
                Console.WriteLine("call f</> ");
                Console.WriteLine("call f</> " + value.GetType());
            }

        }

        [Parameter]
        public string entityName { get; set; }


        public string rnd { get; set; } = "";

        Type cccMetaClass;

        public string label { get; set; }
        string ButtonState = "reload";

        
        Type[] genericArgs = new Type[] { null, typeof(int) };
        private CCC0 obj;
        async Task Click(bool forceLoad=false)
        {
            Console.WriteLine("---");
            
            var f = cccMetaClass.GetMethod(nameof(CCC<IdMapper<int>,int>.f));
            Console.WriteLine("find f" + f.ToString());
            ButtonState = "loading";
            await f.InvokeAsync(obj, new object[] { forceLoad });
            ButtonState = "reload " + obj.getDataSize();
            rnd = entityName;
            StateHasChanged();
            //await load();
        }





        Type gridMetaClass = null;
        



    }
}