# Gesture Library Spec

## 目的

本ドキュメントは、`Unity Enhanced Gesture` の仕様を段階的に整理するためのベース仕様書である。
現時点では、まず外側の構成と責務分担を優先して定義し、各ジェスチャーの細かな判定条件は今後詰めていく。

## ライブラリ概要

- Unity モバイルアプリ向けのタッチジェスチャーライブラリとする
- 入力取得には Unity Input System の `EnhancedTouch` を利用する
- 主な利用対象は uGUI 上の `RectTransform` とする
- 利用者は専用の `MonoBehaviour` をアタッチし、コードから `System.Action<TEvent>` を購読してジェスチャー情報を受け取る

対象ジェスチャーは以下を想定する。

- ドラッグ操作系
- ピンチインアウト系
- タップ判定系

## 利用前提

- 判定エリアは `RectTransform` を基準とする
- 基本的な利用形態は uGUI オブジェクトへの専用コンポーネント付与とする
- `UnityEvent` は採用せず、イベント通知は `System.Action<TEvent>` ベースで提供する
- 利用者はインスペクタ操作よりもコード購読を主軸として扱う想定とする

利用イメージの一例は以下とする。

```csharp
_dragGestureHandler.BeginDragEvent += evt =>
{
    var startPos = evt.StartPosition;
    var pos = evt.Position;
};
```

## 開始条件と UI 利用方針

- ジェスチャーは対象 `RectTransform` の判定エリア内で開始されることを前提とする
- 開始地点に他の uGUI 要素が存在し、それが `RaycastTarget` として入力を受ける場合は、その要素によって開始をブロッキングする
- uGUI は主に `RaycastTarget` を避けるための開始ブロック判定と、効果範囲を `RectTransform` で絞るために利用する
- Handler 間の優先順位を uGUI のヒエラルキー順や描画順へ追従させることは、現時点では仕様責務に含めない

## 排他制御の方針

- ジェスチャーの検出と排他制御は中央集権的に扱う
- 1 つの入力系列に対して同時に複数ジェスチャーを成立させない
- あるジェスチャーが成立した後は、同一入力系列に対する他のジェスチャー成立を停止する
- 排他制御を行う中央管理クラスは `GestureCoordinator` とする
- `GestureCoordinator` は自動生成される中央管理クラスとして設計する想定だが、生成方法の詳細は未確定とする

## アーキテクチャ方針

責務は以下の 4 層に分ける。

- 入力収集層
- 判定対象管理層
- ジェスチャー調停層
- 利用者向け通知層

### 入力収集層

- `EnhancedTouch` からタッチ情報を取得する
- 生の入力をライブラリ内部で扱いやすいスナップショット形式へ正規化する
- この層ではジェスチャー成立判定は行わない

### 判定対象管理層

- `RectTransform` と利用者向けコンポーネントの対応を管理する
- 登録と解除を扱う
- 開始地点でどの対象が入力候補になるかを解決する
- `RaycastTarget` によるブロッキングもここで考慮する
- 同一 `RectTransform` 上では、同型 Handler 群を 1 つの通知グループとして管理する
- 同型 Handler 群に設定差異がある場合は有効な共有設定を解決できないため、不正な構成として扱う

### ジェスチャー調停層

- 進行中の入力系列ごとの状態を管理する
- どの対象がその入力系列を所有するかを決める
- 各判定器へ入力を渡し、最初に成立したジェスチャーを確定する
- 成立後は他判定器の成立を停止する
- キャンセル、終了、指の増減などの状態遷移を一元管理する
- 個別判定に使う設定値は、対象上の同型 Handler 群で共有されている前提で扱う

### 利用者向け通知層

- 利用者がアタッチした `MonoBehaviour` を窓口としてイベントを受け取れるようにする
- 公開 API は内部状態を直接露出せず、イベント引数型を通して必要情報を渡す
- イベントは `System.Action<TEvent>` で公開する
- 同一 `RectTransform` 上に同型 Handler が複数存在する場合、同一ジェスチャーの通知は全 Handler へ同報する

## クラス責務

現時点でのクラス責務は以下を想定する。

### `GestureCoordinator`

- ライブラリ全体の司令塔
- 毎フレームの入力更新を受ける
- 判定対象の一覧を管理する
- 開始地点の候補解決を行う
- `GestureSession` の生成、更新、破棄を行う
- ジェスチャー候補の排他制御を行う

### `GestureHandlerBase : MonoBehaviour`

- 利用者が uGUI オブジェクトへ付与する基底クラス
- 自身が利用する `RectTransform` を提供する
- `GestureCoordinator` への登録、解除を担当する
- 利用者向けイベント公開の土台を持つ

### `DragGestureHandler : GestureHandlerBase`

- ドラッグ系の公開 API を持つ
- `System.Action<DragGestureEvent>` 形式のイベントを公開する
- ドラッグ関連のシリアライズ設定を持つ
- 同一 `RectTransform` 上で複数利用する場合、設定値は他の `DragGestureHandler` と一致している必要がある

### `PinchGestureHandler : GestureHandlerBase`

- ピンチ系の公開 API を持つ
- `System.Action<PinchGestureEvent>` 形式のイベントを公開する
- ピンチ関連のシリアライズ設定を持つ
- 同一 `RectTransform` 上で複数利用する場合、設定値は他の `PinchGestureHandler` と一致している必要がある

### `TapGestureHandler : GestureHandlerBase`

- タップ、ダブルタップ、ロングタップ系の公開 API を持つ
- `System.Action<TapGestureEvent>` 形式のイベントを公開する
- タップ関連のシリアライズ設定を持つ
- 同一 `RectTransform` 上で複数利用する場合、設定値は他の `TapGestureHandler` と一致している必要がある

### `GestureTargetEntry`

- 1 つの判定対象を表す内部管理単位
- `RectTransform` と対応ハンドラー群を型別に持つ
- 同型 Handler 群から共有設定を解決する
- 同型 Handler 群への通知同報を担当する
- 判定対象の管理に必要な情報を持つ

### `GestureSession`

- 1 つの入力系列を表す内部状態
- 開始位置、開始時刻、現在位置、所有対象、確定済みジェスチャー種別などを保持する
- 将来の複数指拡張を受け止める内部状態の中心となる

### `GestureRecognizerBase`

- 内部判定器の基底クラス
- `MonoBehaviour` には依存しない
- 入力スナップショットを受け、成立可否や継続状態を返す
- 対象選択や排他制御の責務は持たない

派生候補は以下とする。

- `DragGestureRecognizer`
- `PinchGestureRecognizer`
- `TapGestureRecognizer`
- `LongPressGestureRecognizer`

### 補助クラス候補

- `GestureRaycastResolver`
  - 開始地点に対する対象解決を担当する
- `GestureInputSnapshot`
  - `EnhancedTouch` から得た入力の内部表現とする
- `DragGestureEvent`
  - ドラッグイベント引数
- `PinchGestureEvent`
  - ピンチイベント引数
- `TapGestureEvent`
  - タップイベント引数

## 処理フロー

### 1. 判定対象の登録

- 利用者が `GestureHandlerBase` 派生コンポーネントを uGUI オブジェクトへ付与する
- コンポーネント有効化時に対象 `RectTransform` が `GestureCoordinator` へ登録される
- `GestureCoordinator` は内部の対象一覧を更新する
- 同一 `RectTransform` 上の同型 Handler は同一通知グループとして登録される
- 同型 Handler 間で設定が一致しない場合、その型のジェスチャーは有効な共有設定を解決できないため無効構成として扱う

### 2. ジェスチャー開始候補の解決

- タッチ開始時に開始地点へ対して候補検索を行う
- 別の `RaycastTarget` が開始地点を占有している場合、その入力系列はライブラリ管理対象にしない
- ライブラリ管理対象の重なりに対する厳密な優先順位制御は、現時点では仕様の主目的に含めない

### 3. 入力系列の所有

- `GestureCoordinator` は候補対象に対して `GestureSession` を生成する
- セッションは対象を 1 つだけ所有する
- 以後の更新は、そのセッション経由で個別判定器へ渡す

### 4. 個別ジェスチャーの評価

- セッション更新ごとに、対象が許可する判定器を評価する
- ドラッグ、タップ、ロングタップ、ピンチなどの候補が待機していても、成立確定は 1 つだけとする
- ある判定器が成立した時点で、その入力系列では他判定器の成立を停止する

### 5. 利用者への通知

- 確定したジェスチャーに応じて対象コンポーネント上のイベントを呼び出す
- 継続通知、終了通知、キャンセル通知も同じコンポーネントへ返す
- 利用者は中央管理クラスを意識せず、ハンドラー経由でイベントを扱う
- 同一 `RectTransform` 上に同型 Handler が複数ある場合、同一イベントはその全 Handler へ返す

## 設定方針

- 判定しきい値や時間設定は、`ScriptableObject` 集約ではなく各ハンドラーの `MonoBehaviour` 側にシリアライズ値として持たせる
- 利用者はインスペクタから各ハンドラーの挙動を直接調整できる
- 初期段階ではローカル設定を優先し、共有設定の仕組みは必要になった段階で追加検討する
- 同一 `RectTransform` 上で同型 Handler を複数使う場合、それらのシリアライズ設定は同一値でそろえる
- 同型 Handler 間で設定差異がある場合、登録順や代表選出で吸収せず不正構成として扱う

想定する設定項目は以下とする。

- ドラッグ開始しきい値
- タップ許容移動量
- ダブルタップ判定時間
- ロングタップ判定時間
- ピンチ開始しきい値
- 将来的なフリック判定しきい値

## 対象ジェスチャーの現時点の整理

### ドラッグ操作系

- 開始には移動しきい値を設ける
- しきい値を超えるまではドラッグ成立とみなさない
- 継続的な移動量を取得できること
- フリック判定へ転用しやすい情報を取得できること
- 代表的なイベントとして `BeginDragEvent` のような開始イベントを想定する
- イベント引数には少なくとも開始位置と現在位置を含める

### ピンチインアウト系

- 2 点間距離の変化を取得できること
- 角度情報を取得できること

### タップ判定系

- シングルタップを扱える前提で設計する
- ダブルタップを判定対象に含める
- ロングタップを判定対象に含める

## 現時点での判断メモ

- `EnhancedTouch` 前提で実装を進める
- 公開 API の命名は `Target` より `Handler` を優先する
- 中央管理クラス名は `GestureCoordinator` とする
- まずは全パターンを広げず、責務分割を先に固める
- uGUI は開始ブロック判定と効果範囲制限のために利用し、Handler 間優先順位の再現までは担わない

## 未確定事項

- `GestureCoordinator` の自動生成方法
- ドラッグ、ピンチ、タップそれぞれの詳細な成立条件
- イベント引数型に含める項目の詳細
- 複数指操作中の優先順位とキャンセル条件
- `GestureSession` を 1 指中心で持つか、最初から複数指抽象化で持つか
- ロングタップ後ドラッグのようなジェスチャー遷移をどう扱うか
- 公開イベント命名を `BeginDragEvent` 系で揃えるか、別命名へ寄せるか

## 再設計メモ

- まずは入力取得と内部利用向けデータ変換を最優先で実装する
- `EnhancedTouch` の解釈と正規化は `GestureCoordinator` の責務として扱う
- 判定対象そのものは `RectTransform` 基準とし、`RaycastTarget` は開始ブロック判定にのみ使う
- 判定対象自身が `RaycastTarget` であることを必須条件にしない
- Unity Editor 上でマウス操作による検証ができることを前提にする
- Editor では `TouchSimulation` を用いた検証経路を用意する
- 最初は `Drag` だけを通す最小構成で作り、そこから段階的に `Tap` や `Pinch` を追加する
- 初期段階ではクラス数を増やしすぎず、`GestureCoordinator` に寄せた薄い実装から始める
- 先回りした抽象化や拡張前提の分割は避け、動作確認後に必要な分だけ整理する
- `GestureCoordinator` は `RectTransform` そのものを知る必然は薄く、必要なのは入力座標と各 Handler への問い合わせ経路である
- 座標をどう判定するか、`RectTransform` を使うかどうかは Handler 側の都合として扱う
- 開始時には候補 Handler 群を集め、その中から最終的な配送先 Handler を 1 つだけ選ぶ
- 複数 Handler が候補になる場合は、`Priority` の数値が最も高い Handler を優先する
- `Priority` が同値の場合は登録順で決定する
- 開始時にイベント送信対象となった Handler は `GestureCoordinator` が入力系列単位で保持する
- `Drag` の開始イベントを受けていない Handler に、`Drag` 中イベントや終了イベントを送らない
- 開始イベントを送った Handler には、終了またはキャンセルまで後続イベントを確実に送る
- 後続イベントの配送先管理は `GestureCoordinator` の責務とする
- 解析機はイベントデータ生成に寄せ、どの Handler へ配送するかの判断は `GestureCoordinator` 側で扱う
- `GestureCoordinator` は生の `EnhancedTouch` や `Mouse` を直接解釈せず、入力解析インターフェースへ処理を委譲する
- 入力解析実装は、実機向け `EnhancedTouch` 系と Editor 向け入力系で差し替え可能にする
- 入力解析結果は共通の内部入力データへ正規化し、その後の `Recognizer` と `Handler` 判定は共通処理に寄せる
- Handler 探索や保持は concrete class ではなく、Handler 用インターフェース型を通して行う
- `GestureCoordinator` は自動生成せず、シーンへ明示的に配置された場合のみ動作する形を優先する
- `GestureCoordinator` は `Instance` アクセス時に自動生成するシングルトン設計を採用しない
- `static` アクセスは許容してよいが、インスタンスの生存は `MonoBehaviour` として配置された実体に依存させる
- `EnhancedTouchSupport` や `TouchSimulation` の有効化責務は固定せず、`GestureCoordinator` の設定で自動管理するか外部管理するかを選べるようにする
- 既定値は `GestureCoordinator` による自動 On/Off 管理とし、必要な場合のみ外部管理へ切り替えられるようにする
- 入力更新の駆動方式は既定で Unity の `Update` を使い、必要な場合は `ManualUpdate` へ切り替えられるようにする
- `ManualUpdate` は 1 フレームにつき 1 回だけ呼ぶ前提とし、同一フレームでの重複呼び出しは例外として扱ってよい
- 外部管理モードで入力系が未初期化のまま `GestureCoordinator` が動作した場合は警告を出す
- Handler は `GestureCoordinator` へ自動登録する形を基本とし、基底クラス側で登録処理を担う
- `CanHandle` に渡す情報は `ScreenPosition` を基本とし、`Camera` や `Ray`、入力 phase などは必須前提にしない
- `Pinch` の Editor シミュレーションは `Alt +` マウスドラッグを基本とする
- `Pinch` シミュレーションでは、ドラッグ開始位置を中央点、現在位置を指 1、開始位置に対する点対称位置を指 2 として扱う
- 解析機は型ごとにユニークインスタンス管理し、必要に応じてジェスチャー種別専用の入力解析を行えるようにする
- `Recognizer` は `GestureCoordinator` 側の固定資産として保持し、Handler ごとに生成しない
- `Recognizer` の解決は `GestureCoordinator` が行い、`Type` ベースの反射生成は使わない
- `RectTransform` を対象にする `Drag` Handler は、uGUI 前提であることが分かる命名にする
- `RectTransform` 版は `Canvas` 文脈のカメラを使い、`GestureCoordinator` の共有カメラには依存しない
- `Collider` を対象にする別の `Drag` Handler を用意し、こちらは `GestureCoordinator` の共有カメラを使って判定する
- `GestureCoordinator` の共有カメラは外部へ `public` 公開せず、必要な情報は `GestureEvent` 側へ載せて利用する
