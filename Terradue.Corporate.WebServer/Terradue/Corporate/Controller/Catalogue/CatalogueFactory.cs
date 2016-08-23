using System;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
    public class CatalogueFactory {

        public IfyContext Context { get; set; }

        public CatalogueFactory(IfyContext context) {
            this.Context = context;
        }

        public bool IndexExists(string index){
            return false;    
        }

        public void IndexCreate(string index){
            if(!IndexExists(index)){
                
            }
        }
    }
}

