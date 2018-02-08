using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.SolrProvider;
using System.Globalization;
using System.Reflection;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Linq;

namespace Sitecore.Support.ContentSearch.SolrProvider
{
    public class SolrFieldNameTranslator : Sitecore.ContentSearch.SolrProvider.SolrFieldNameTranslator
    {
        private FieldInfo currentCultureCode = typeof(Sitecore.ContentSearch.SolrProvider.SolrFieldNameTranslator).GetField("currentCultureCode", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo fieldMap = typeof(Sitecore.ContentSearch.SolrProvider.SolrFieldNameTranslator).GetField("fieldMap", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo schema = typeof(Sitecore.ContentSearch.SolrProvider.SolrFieldNameTranslator).GetField("schema", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo index = typeof(Sitecore.ContentSearch.SolrProvider.SolrFieldNameTranslator).GetField("index", BindingFlags.NonPublic | BindingFlags.Instance);        

        public SolrFieldNameTranslator(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex solrSearchIndex) : base(solrSearchIndex)
        {
        }
        public override string GetIndexFieldName(string fieldName)
        {
            return this.GetIndexFieldName(fieldName, (CultureInfo)null);
        }
        public string GetIndexFieldName(string fieldName, CultureInfo culture)
        {
            return this.ProcessFieldName(fieldName, null, culture, null, true);
        }

        private string ProcessFieldName(string fieldName, Type returnType, CultureInfo culture, string returnTypeString = "", bool aggressiveResolver = false)
        {
            var strippedFieldName = this.StripKnownExtensions(fieldName);

            strippedFieldName = strippedFieldName.Replace(" ", "_").ToLowerInvariant();

            var cultureCode = string.Empty;

            cultureCode = culture != null ? culture.TwoLetterISOLanguageName : (string)this.currentCultureCode.GetValue(this);

            // Check there is no match with a field name in the fieldmap.
            var configurationByName = ((SolrFieldMap)this.fieldMap.GetValue(this)).GetFieldConfiguration(strippedFieldName) as SolrSearchFieldConfiguration;

            if (configurationByName != null)
            {
                return configurationByName.FormatFieldName(strippedFieldName, (SolrIndexSchema)this.schema.GetValue(this), cultureCode, null);
            }

            if (((SolrIndexSchema)this.schema.GetValue(this)).AllFieldNames.Contains(strippedFieldName))
            {
                return strippedFieldName;
            }

            SolrSearchFieldConfiguration configurationByType = null;

            if (returnType != null)
            {
                configurationByType = ((SolrFieldMap)this.fieldMap.GetValue(this)).GetFieldConfiguration(returnType) as SolrSearchFieldConfiguration;
            }

            if (!string.IsNullOrEmpty(returnTypeString))
            {
                configurationByType = ((SolrFieldMap)this.fieldMap.GetValue(this)).GetFieldConfigurationByReturnType(returnTypeString) as SolrSearchFieldConfiguration;
            }

            if (configurationByType != null)
            {
                return configurationByType.FormatFieldName(strippedFieldName, (SolrIndexSchema)this.schema.GetValue(this), cultureCode, null);
            }

            if (aggressiveResolver)
            {
                #region Sitecore.Support.94802
                foreach (SolrSearchFieldConfiguration configuration in ((SolrFieldMap)this.fieldMap.GetValue(this)).GetAvailableTypes())
                {
                    string str = configuration.FieldNameFormat.Replace("{0}", string.Empty);
                    if (fieldName.EndsWith(str, StringComparison.Ordinal))
                    {
                        return fieldName;
                    }
                }
                #endregion
                var res = FindTemplateField(strippedFieldName);                

                if (res.Any())
                {
                    var configurationByFieldType = ((SolrFieldMap)this.fieldMap.GetValue(this)).GetFieldConfigurationByFieldTypeName(res.First().Type) as SolrSearchFieldConfiguration;

                    if (configurationByFieldType != null)
                    {
                        #region Sitecore.Support.94802
                        //Add to cache..
                        //((SolrFieldMap)this.fieldMap.GetValue(this)).AddFieldByFieldName(strippedFieldName, configurationByFieldType);
                        #endregion
                        return configurationByFieldType.FormatFieldName(strippedFieldName, (SolrIndexSchema)this.schema.GetValue(this), cultureCode, null);
                    }
                }
            }

            return fieldName.ToLowerInvariant();
        }
        internal virtual IQueryable<TemplateResolver> FindTemplateField(string fieldName)
        {
            var q = ((SolrSearchIndex)this.index.GetValue(this)).CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck).GetQueryable<TemplateResolver>();
            return q.Where(x => x.Name == fieldName).Filter(x => x.TemplateName == "Template field").Take(1);
        }
    }
}