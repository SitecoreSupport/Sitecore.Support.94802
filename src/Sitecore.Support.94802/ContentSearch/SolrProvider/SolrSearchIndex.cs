using Sitecore.ContentSearch.Maintenance;
using System.Reflection;

namespace Sitecore.Support.ContentSearch.SolrProvider
{
    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex
    {
        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) :
          this(name, core, propertyStore, null)
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            FieldNameTranslator = new Sitecore.Support.ContentSearch.SolrProvider.SolrFieldNameTranslator(this);          
        }
    }
}