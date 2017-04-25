namespace $safeprojectname$.Core
{
    public static class DecimalExtentions
    {
        public static string FormatMoney(this decimal amount)
        {
            return amount.ToString("C2");
        }

        public static string FormatMoneyWithCurrency(this decimal amount)
        {
            return amount.ToString("##,###,###,##0.00") + "  AUD";
        }
    }
}