namespace CsoftRabbitMq
{
    public class GeneralQueueResponse<T>
    {
        /// <summary>
        /// Sonuç değeri (T Property)
        /// </summary>
        public T? Result { get; set; }
        /// <summary>
        ///  Eğer hata var ise içeriğini döner.
        /// </summary>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// Eğer hata mesajı var ise true döner.
        /// </summary>
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
    }
}
