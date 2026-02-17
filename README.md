# PoseAudioInteraction

## 概要

**PoseAudioInteraction**は、Mediapipeを用いたリアルタイム姿勢推定に基づき、ユーザーの体の動きに応じて立体音響（HRTF）を制御するUnityプロジェクトです。Webカメラからリアルタイムで姿勢を推定し、骨格をオーバーレイ表示しながら、音源位置をリアルタイムに制御します。

## 特徴

- **リアルタイム姿勢推定**: Mediapipe PoseLandmarkerによる33点の骨格検出
- **骨格オーバーレイ表示**: 推定した骨格をWebカメラ映像上にリアルタイム描画
- **HRTF（頭部伝達関数）による立体音響**: Steam Audioを使用した高品質な3D音響
- **CSVデータ駆動**: 事前に記録した姿勢推定データを再生して音源を制御
- **複数の制御モード**: 顔の向き、手首位置、肩の位置など様々なデータに対応
- **日本語UI対応**: Noto Sans JP フォントによる日本語テキスト表示

## プロジェクト構成

```
Assets/
├── Scenes/
│   └── MainScenes.unity          # メインシーン（全Canvas統合）
├── Scripts/
│   ├── Controllers/              # 入出力制御
│   │   ├── PoseEstimator.cs      # Mediapipeリアルタイム姿勢推定
│   │   ├── PoseOverlayRenderer.cs # 骨格オーバーレイ描画
│   │   ├── VideoController.cs    # 元動画の再生制御（動的解像度対応）
│   │   └── WebCamDisplay.cs      # Webカメラ映像表示
│   ├── Managers/                 # シーン・状態管理
│   │   └── SceneNavigator.cs     # Canvas切り替えによる画面遷移
│   ├── UI/                       # 各画面のUI管理
│   │   ├── HomeManager.cs        # ホーム画面（トレーニング選択）
│   │   ├── TrainingManager.cs    # トレーニング画面
│   │   └── ResultManager.cs      # リザルト画面
│   ├── NEDO02.cs                 # 顔向き(Yaw)データ連動
│   ├── NEDO06.cs                 # 両手首位置データ連動
│   ├── NEDO46.cs                 # 肩データ追従
│   ├── NEDOBase.cs               # NEDO基底クラス
│   ├── NEDOSettings.cs           # 設定管理（ScriptableObject）
│   └── CircleMovement.cs         # 円周移動（連続/離散、水平/垂直）
├── Fonts/                        # 日本語フォント（Noto Sans JP）
├── StreamingAssets/
│   ├── CSV/                      # 姿勢推定CSVデータ
│   │   ├── NEDO02.csv            # 顔向きデータ
│   │   ├── NEDO06.csv            # 手首位置データ
│   │   └── NEDO46.csv            # 肩位置データ
│   ├── pose_landmarker_*.bytes   # Mediapipeモデルファイル
│   └── *.mp4 / *.mov             # トレーニング元動画
└── Plugins/
    └── SteamAudio/               # Steam Audioプラグイン
```

## 画面フロー

```
Canvas_Home（ホーム画面）
  ├── NEDO02 ボタン ──→ Canvas_Training（トレーニング画面）──→ Canvas_Result（リザルト画面）
  ├── NEDO06 ボタン ──→          〃                                    │
  └── NEDO46 ボタン ──→          〃                                    │
                                                                       ↓
                                                              ホームに戻る / リトライ
```

※ Canvas切り替え方式（`SetActive`）により、1つのシーン内で画面を切り替えます。

## 技術スタック

| 技術 | 用途 |
|------|------|
| **Unity 2022.3.25f1** | 3D環境と音響制御 |
| **C#** | スクリプト記述 |
| **Mediapipe Unity Plugin v0.16.3** | リアルタイム姿勢推定 |
| **Steam Audio** | HRTF/立体音響処理 |
| **TextMeshPro + Noto Sans JP** | 日本語UI表示 |

## 使用プラグイン

- **Mediapipe Unity Plugin**: リアルタイム姿勢推定（PoseLandmarker, 33点骨格検出）
- **Steam Audio**: 3D音響環境を構築するためのオーディオプラグイン（HRTF対応）
- **Native WebSocket**: WebSocket通信（リアルタイムデータ送受信用）
- **Newtonsoft.Json**: JSONデータのシリアライズ/デシリアライズ

## セットアップ

### 1. クローン

```bash
git clone git@github.com:ISSE0116/PoseAudioInteraction.git
cd PoseAudioInteraction
git submodule update --init --recursive
```

### 2. Steam Audioのインストール

1. [Steam Audio公式サイト](https://valvesoftware.github.io/steam-audio/downloads.html)から最新版をダウンロード
2. 解凍して`Assets/Plugins/SteamAudio`フォルダにコピー

### 3. Mediapipe Unity Pluginのインストール

1. [MediaPipeUnityPlugin Releases](https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3)から`MediaPipeUnityPlugin-all.zip`をダウンロード
2. 解凍して`.unitypackage`をUnityにインポート（`Assets > Import Package > Custom Package`）

### 4. SOFAファイル（HRTF）

カスタムHRTFを使用する場合は、SOFAファイルを`Assets/`フォルダに配置し、Steam Audio Settingsで登録してください。

## 使用方法

1. **シーンを開く**: `Assets/Scenes/MainScenes.unity`
2. **Playモードで実行**: ホーム画面が表示される
3. **トレーニングを選択**: NEDO02/06/46ボタンをクリック
4. **トレーニング実行**: 元動画・CSVデータ再生が開始、Webカメラで骨格がリアルタイム表示
5. **結果確認**: リザルト画面でスコアを確認、ホームに戻るかリトライ

## テスト

### 実行方法

1. Unityエディターで `Window > General > Test Runner` を開く
2. `EditMode` タブを選択
3. `Run All` をクリック

### テスト内容

| テスト名 | 説明 |
|----------|------|
| `TryParseFloat_ValidNumber` | 正常な数値パースの検証 |
| `TryParseFloat_None` | "None"文字列の処理検証 |
| `TryParseFloat_EmptyString` | 空文字列の処理検証 |
| `CalculateFrame_At60Fps` | 60fpsでのフレーム計算検証 |
| `CalculateFrame_At30Fps` | 30fpsでのフレーム計算検証 |

## ライセンス

MIT License
