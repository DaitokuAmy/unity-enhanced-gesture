using System.Collections.Generic;

namespace UnityEnhancedGesture {
    /// <summary>
    /// 入力取得と解析結果生成を担う共通契約
    /// </summary>
    internal interface IGestureInputProvider {
        /// <summary>
        /// 未準備時の警告文
        /// </summary>
        string NotReadyMessage { get; }

        /// <summary>
        /// 入力処理可能かどうか
        /// </summary>
        /// <param name="inputManagementMode">入力管理方式</param>
        /// <returns>処理可能な場合は true</returns>
        bool IsReady(GestureInputManagementMode inputManagementMode);

        /// <summary>
        /// 有効化時処理
        /// </summary>
        /// <param name="inputManagementMode">入力管理方式</param>
        void Enable(GestureInputManagementMode inputManagementMode);

        /// <summary>
        /// 無効化時処理
        /// </summary>
        /// <param name="inputManagementMode">入力管理方式</param>
        void Disable(GestureInputManagementMode inputManagementMode);

        /// <summary>
        /// 現在フレームの入力一覧を収集
        /// </summary>
        /// <param name="results">格納先バッファ</param>
        void CollectInputs(List<GesturePointerInput> results);
    }
}
