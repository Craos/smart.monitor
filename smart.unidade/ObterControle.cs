using System.Windows.Forms;

namespace smart.controles
{
    public static class ObterControle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired(this Control control, MethodInvoker action)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch
            {

            }
        }
    }
}
