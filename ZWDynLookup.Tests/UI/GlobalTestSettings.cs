using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZWDynLookup.Tests
{
    /// <summary>
    /// 包含所有测试类别的全局测试设置
    /// </summary>
    public static class GlobalTestSettings
    {
        /// <summary>
        /// 测试环境标识
        /// </summary>
        public const string TestEnvironment = "CAD_UI_Test";

        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
        public const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// 截图保存目录
        /// </summary>
        public const string ScreenshotDirectory = "TestScreenshots";

        /// <summary>
        /// 测试数据目录
        /// </summary>
        public const string TestDataDirectory = "TestData";
    }
}