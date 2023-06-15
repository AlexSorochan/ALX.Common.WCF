using System.ComponentModel;

namespace ALX.Common.WCF
{
    public class Enums
    {
        /// <summary>
        /// Способы аутентификации
        /// <list type="table">
        ///     <item>
        ///         <term>0 = NoAuth</term>
        ///         <description>Без аутентификации</description>
        ///     </item>
        ///     <item>
        ///         <term>1 = Certificate</term>
        ///         <description>Сертификат</description>
        ///     </item>
        ///     <item>
        ///         <term>2 = UserName</term>
        ///         <description>Имя пользователя и пароль</description>
        ///     </item>
        /// </list>
        /// </summary>
        public enum LoginMethods
        {
            /// <summary>
            /// Без аутентификации
            /// </summary>
            [Description("Без аутентификации")] NoAuth = 0,
            /// <summary>
            /// Сертификат
            /// </summary>
            [Description("Сертификат")] Certificate = 1,
            /// <summary>
            /// Логин и пароль
            /// </summary>
            [Description("Имя пользователя и пароль")] UserName = 2
        }
    }
}
