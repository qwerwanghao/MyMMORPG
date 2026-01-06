namespace GameServer.Properties
{
    /// <summary>
    /// 设置类：处理应用程序设置的特定事件
    /// </summary>
    /// <remarks>
    /// 可用事件：
    /// - SettingChanging：在更改设置值之前引发
    /// - PropertyChanged：在更改设置值之后引发
    /// - SettingsLoaded：在加载设置值之后引发
    /// - SettingsSaving：在保存设置值之前引发
    /// </remarks>
    public sealed partial class Settings
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Settings()
        {
            // // 若要为保存和更改设置添加事件处理程序，请取消注释下列行:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        /// <summary>
        /// 设置更改前事件处理程序
        /// </summary>
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // 在此处添加用于处理 SettingChangingEvent 事件的代码。
        }

        /// <summary>
        /// 设置保存前事件处理程序
        /// </summary>
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 在此处添加用于处理 SettingsSaving 事件的代码。
        }
    }
}
