## 概要

**PoseAudioInteraction**は、Mediapipeを用いた姿勢推定に基づき、ユーザーの体の動きに応じて立体音響を制御するシステムの予定です。ユーザーが音源に近づくと音が大きくなり、遠ざかると音が小さくなる動的な音響環境を提供します。

## 特徴

- **リアルタイム姿勢推定**: Mediapipeを使用して、ユーザーの体のキーポイントをリアルタイムで検出。
- **動的音響制御**: ユーザーの体の位置に基づいて音量を動的に調整。
- **インタラクティブな3D音響環境**: 音源がユーザーの動きに適応して、よりリアルな立体音響を提供。

## 技術

- **Unity**: 3D環境と音響制御のために使用。
- **C#**: Unityスクリプトの記述に使用。
- **Mediapipe**: リアルタイム姿勢推定のために使用。

## 使用したプラグイン

- **Native WebSocket**: WebSocket通信を利用してリアルタイムデータを送受信し、音響制御や姿勢推定のデータを同期。
- **Steam Audio**: 3D音響環境を構築するためのオーディオプラグイン。
- **Newtonsoft.Json**: JSONデータのシリアライズとデシリアライズを行うために使用。

## クローン手順

このプロジェクトにはサブモジュールが含まれているため、リポジトリをクローンした後、以下のコマンドを実行してサブモジュールを初期化し、必要な依存関係を取得する必要があります。

```bash
git clone https://github.com/ISSE0116/PoseAudioInteraction.git 
cd PoseAudioInteraction
git submodule update --init --recursive
```

## Steam Audioのインストール手順

このプロジェクトでは、3D音響処理のために`Steam Audio`プラグインが必要です。リポジトリには含まれていないため、以下の手順で個別にインストールしてください。

1. [Steam Audioの公式サイト](https://valvesoftware.github.io/steam-audio/downloads.html)から最新版をダウンロードします。
2. ダウンロードしたZIPファイルを解凍し、`Assets/Plugins/SteamAudio`フォルダにコピーします。
