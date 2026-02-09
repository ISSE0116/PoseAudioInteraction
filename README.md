# PoseAudioInteraction

## 概要

**PoseAudioInteraction**は、Mediapipeを用いた姿勢推定データに基づき、ユーザーの体の動きに応じて立体音響（HRTF）を制御するUnityプロジェクトです。CSVファイルに記録された姿勢データを再生し、音源位置をリアルタイムに制御します。

## 特徴

- **HRTF（頭部伝達関数）による立体音響**: Steam Audioを使用した高品質な3D音響
- **CSVデータ駆動**: 事前に記録した姿勢推定データを再生して音源を制御
- **複数の制御モード**: 顔の向き、手首位置、肩の位置など様々なデータに対応

## プロジェクト構成

```
Assets/
├── Scripts/                    # C#スクリプト
│   ├── NEDO02.cs              # 顔向き(Yaw)データ連動
│   ├── NEDO06.cs              # 両手首位置データ連動
│   ├── NEDO46.cs              # 肩データ追従
│   ├── CircleMoveContinuous.cs # 連続的な円周移動
│   ├── MoveOnCircle_2.cs      # 45度刻みの離散移動
│   └── MoveOnCircle3.cs       # Steam Audio対応版
├── StreamingAssets/
│   └── CSV/                   # 姿勢推定CSVデータ
│       ├── face_orientation.csv
│       ├── relative_wrist_to_nose.csv
│       └── shoulder_center.csv
└── Plugins/
    └── SteamAudio/            # Steam Audioプラグイン
```

## 技術スタック

| 技術 | 用途 |
|------|------|
| **Unity** | 3D環境と音響制御 |
| **C#** | スクリプト記述 |
| **Steam Audio** | HRTF/立体音響処理 |
| **Mediapipe** | 姿勢推定（データ生成時） |

## 使用プラグイン

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

### 3. SOFAファイル（HRTF）

カスタムHRTFを使用する場合は、SOFAファイルを`Assets/`フォルダに配置し、Steam Audio Settingsで登録してください。

## 使用方法

1. **シーンを開く**: `Assets/Scenes/MainScenes.unity`
2. **スクリプトを設定**: NEDOオブジェクトにNEDO02/06/46スクリプトをアタッチ
3. **Inspectorで設定**:
   - `Sound Source`: 音源のTransform
   - `Listener`: リスナー（カメラ）のTransform
   - `Audio Source`: AudioSourceコンポーネント
   - `Button`: 再生開始ボタン
4. **Playモードで実行**: ボタンをクリックしてCSVデータ再生を開始

## ライセンス

MIT License
