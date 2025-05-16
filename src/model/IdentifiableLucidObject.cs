namespace LucidStandardImport.model
{
    public interface IIdentifiableLucidObject
    {
        public string Id { get; set; }

        /// <summary>
        /// Optional external Id field to keep in sync with the internal generated Ids.
        /// </summary>
        public string ExternalId { get; set; }
    }
}
