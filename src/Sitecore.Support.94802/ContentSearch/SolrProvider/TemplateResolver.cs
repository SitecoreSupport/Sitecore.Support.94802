namespace Sitecore.ContentSearch.SolrProvider
{
    internal class TemplateResolver
    {
        [IndexField("_name")]
        public string Name { get; set; }

        [IndexField("_templatename")]
        public string TemplateName { get; set; }

        public string Type { get; set; }
    }
}