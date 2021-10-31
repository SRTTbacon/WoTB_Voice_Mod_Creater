# WoTB_Voice_Mod_Creater
Wwiseの使い方がわからなくても自動でプロジェクトを作成、ビルド、適応をしてくれるソフトです。<br>
おまけとして戦闘BGMの作成と、謎の音楽プレイヤーを搭載しています。<br>
既にオンラインも実装していますが、バグがまだまだたくさんあるため利用できないようになっています。<br>
操作方法はYoutubeの動画を参照してください<br>
バグや不具合、わからないことがあればSRTTbacon#2395までご連絡を！<br>
V1.2.7から.NetFramework4.6.1に変更しましたので、Windows7などはマイクロソフトからダウンロード&インストールする必要があります。<br>
<br>
ダウンロード:https://drive.google.com/file/d/1rTmmRGbGPCuXOSDNt31pz6hQ1cqGWWb1/view<br>
<br>
ソース内のWwise_Class/V1とV2の違いは、V1では.pckファイルを扱い、V2では.bnkファイルを扱う感じです。<br>
<br>
# V1.4.6<br>
・Wwise Playerが正常に機能しない問題を修正<br>
・音楽プレイヤーのランダム再生中、リスト内の曲が均等に再生されるよう変更<br>
(再生済みになると文字が灰色になります。)<br>
・音声部分と楽器部分を分ける機能で、サーバー処理でなくクライアント側で処理できる機能を追加<br>
・SEの音源を変更できる機能を追加<br>
・音声制作にて、ウィンドウバーを表示させるとクリアや色の変更の項目が消えてしまう問題を修正<br>
・その他細かなバグを修正<br>
<br>
V1.4.5<br>
・ソフト内でModの動作確認ができる機能を追加 (Wwiseの機能をMod Creater内で使用できるようになりました)<br>
・サウンド編集にて、正常にロードされない場合がある問題を修正<br>
・.bnkの音量を変更できる機能を追加 (Thanks to Leo/AMX)<br>
・音楽プレイヤーにて、時間が長い動画を再生しているときの音ズレを改善(V1.4.3の内容を改良)<br>
・音声Modを作成する際、適応後にWoTBを自動で起動できるオプションを追加(Thanks to syuu__)<br>
・その他細かなバグを修正や一部仕様を変更<br>
<br>
V1.4.4<br>
・砲撃音を作成できる機能を追加('その他のサウンドMod'の2ページ目に含まれています)<br>
・サーバーの仕様変更<br>
・PCにSteam版WoTBがインストールされていなくてもModを作成できるように変更(FMOD時代の仕様が残っていました)<br>
・細かなバグを修正 & 一部仕様を変更<br>
<br>
V1.4.3<br>
・サーバーのセキュリティを大幅に強化(第三者からの不正アクセスを防止します)<br>
・音楽プレイヤーにて、曲のボーカルと楽器を分ける機能を追加<br>
・タイトルバーを表示できるオプションを追加(Thanks to goddy_516)<br>
・音楽プレイヤーにて、時間が長い動画を再生しているときに音ズレが訪れていた問題を修正<br>
・約15秒に1回のタイミングで0.5秒ほどフリーズしていた問題を修正<br>
・チャットを送信しても画面内のチャット欄が更新されない問題を修正<br>
<br>
V1.4.2<br>
・音声Mod作成の際、項目に音声が入っていない場合標準のボイスを再生できるようにするオプションを追加<br>
・被弾時の音声を作成できる機能を追加(貫通、非貫通の音声をわけることができます)<br>
・サウンド編集のオプションを追加<br>
・音声制作画面で、チャット時の項目を追加(Thanks to PKF_yoshi)<br>
・サウンド編集の設定画面にて、チェックボックスのデザインを変更(見やすくなります)<br>
・サウンド編集のショートカットキーがウィンドウが前面にない状態でも反応することがある問題を修正<br>
・サウンド編集にて、速度と音量を変更できるショートカットキーを追加<br>
・サウンド編集にて、10分以上のファイルを読み込むときに、波形の生成に時間がかかるため事前に用意した画像を使用するように変更<br>
・サウンド編集にて、内容をセーブ、ロードできる機能を追加<br>
・一部仕様の変更や細かなバグを修正<br>
<br>
V1.4.0<br>
・.bnk + .pckファイルからWwiseのプロジェクトファイルを作成できる機能を追加(世界初！)<br>
・サウンドファイル(.wav又は.mp3)を編集できる機能を追加((Thanks to PKF_yoshi))<br>
・音声作成時や変換時など、時間を要するときに作業が完了したことをタスクバーに通知させる機能を追加(アイコンを光らせるやつです)<br>
・ホーム画面に.wvsまたは.wmsファイルをドラッグすると自動でロードしてくれる機能を追加<br>
    また、音楽プレイヤーに対応する拡張子のファイルをドラッグするとリストに追加されます<br>
・ホーム画面から、変更履歴(ChangeLog)を確認できる機能を追加<br>
・Youtubeの動画を取得するときに処理が停止してしまっていた問題を修正(音声のみの場合は問題ありませんでした)<br>
・音楽プレイヤーにて、Youtubeの動画を取得してソフトを再起動した際リストが削除されてしまう問題を修正<br>
・音楽プレイヤーにて、同名の曲を追加するとき、警告がでるにもかかわらずリストに追加されていた問題を修正<br>
・処理を実行中にソフトがクラッシュした場合、確率でWindowsを再起動するまでソフトが立ち上がらなくなる問題を修正(致命的スギィ-)<br>
・一部の.bnkファイルを解析するときにクラッシュしてしまう問題を修正<br>
・dvpl解除とdvpl化の機能にて、フォルダを指定できるオプションを追加(Thanks to yurina_taki)<br>
    また、変換前のファイルを変換後に削除するか指定できるようになります。<br>
・リザルト時に音声を再生できるように変更(Thanks to yurina_taki)<br>
・ロード|リザルトBGMの画面で、WoTBに反映される音量バーを追加(Thanks to こみやかほ)<br>
・Youtubeから取得するときに、.mp3形式で保存されるように変更(Thanks to PKF_yoshi)<br>
・戦闘BGMが予期せぬタイミングで再生される可能性があったためreload.bnkにBGMを入れるオプションを削除<br>
・その他細かなバグの修正や一部仕様の変更<br>
<br>
V1.3.9<br>
・チャットのレイアウトを変更(少しは見やすくなったかな？)<br>
・音楽プレイヤーで、リストを変更できる機能を追加(Shift+1～9のキーを押すと、リストが切り替わります。終了した地点のリストが起動時に開かれます。)<br>
また、終了地点のリストが空かつ1番目のリストが空でない場合は1番目のリストが選択されます。<br>
・ログインまたはアカウント登録時にショートカットキーと被って一部の大文字のローマ字が打てなかった問題を修正<br>
・一部のbnkファイルが正常に解析されなかった問題を修正(ActorMixerが取得できていませんでした)<br>
・その他細かなバグを修正<br>
<br>
V1.3.8<br>
・上級者向けに、イベントIDごとにファイルを抽出できる機能を追加(ホーム画面でShift + Eキーを押すと画面が表示されます)<br>
・解析できないbnkファイルがあった問題を修正<br>
・WoTから変換する際、audio_mods.xmlを指定できる機能を追加(これで、イベント名が変更されていた場合でも反映されるようになります)<br>
・音楽プレイヤーにて、バックグラウンド再生のチェックを切った状態で画面を閉じると曲が再生されたままになる問題を修正<br>
・Youtubeの仕様変更により動画が取得できなかった問題を修正<br>
・音楽プレイヤーにて、ランダム再生時でも再生時間を指定してその間をループできるように変更<br>
・その他細かなバグの修正や仕様の変更<br>
<br>
V1.3.7<br>
・リザルト時、優勢時のBGMModを作成できるように変更<br>
・"ロード|リザルトBGM"画面の左側のリストにて、CtrlまたはShiftキーを押しながらクリックすると複数選択できるように変更(選択した項目をすべてビルドできるようになります)<br>
・音楽プレイヤーの画面で、Shift + Dキーを押すと追加されている曲をクリアするダイアログを表示させるように<br>
・"音量を均一にする"にチェックを入れてビルドすると音声が反映されない問題を修正<br>
・一部のbnkファイルが読み込めない問題を修正(完全ではないかもしれません。)<br>
・一部の環境にて、戦闘BGMが反映されなかった問題を修正<br>
・その他細かなバグの修正や一部仕様の変更<br>
<br>
V1.3.6<br>
・ソフト内で戦闘開始前のロードBGMModを作成できる機能を追加<br>
・ソフト内でアカウント登録またはログインをしないと各機能を利用できないように仕様を変更<br>
(利用人数を把握したいため、ご協力ください。ユーザー名とパスワードはお好みで指定していただいて構いません。)<br>
・その他一部仕様を変更、細かなバグを修正<br>
<br>
V1.3.5<br>
・FSBまたはBNKファイルからこのソフト専用のセーブファイルを作成できる機能を追加<br>
(これで既存のModを改造できるようになりますが、作者以外の配布はしないでください。)<br>
・音楽プレイヤーにて、停止している状態から別の曲に移行すると音量や速度が変更できなくなる問題を修正<br>
・BlitzからWoTに変換した際、WoTへのインストール先が正しく取得できていなかった問題を修正<br>
・その他細かなバグの修正や仕様の変更<br>
<br>
V1.3.4<br>
・戦闘BGMをreload.bnkに含めるように変更　これにより、戦闘に入り遅れた場合リロード完了時にBGMが再生されるようになります(重複はしません)<br>
また、時間内に戦闘に入れた場合はリロード完了を待たなくても再生されます。<br>
・SHift+Pキーで一時フォルダの場所を確認できるように変更<br>
・細かなバグを修正<br>
<br>
V1.3.3<br>
・"WoTから移植"にて、指定した音声の移植を無効化できる機能を追加<br>
・WoTBからWoTに移植する際、PCにWoTがインストールされていたら自動で導入するか確認するメッセージを追加<br>
・音声制作ツール&SEの設定画面にて、再生デバイスが正常に反映されていない問題を修正<br>
・音声制作ツールにて、選択したのに色が変わらない場合がある問題を修正<br>
<br>
V1.3.2<br>
・WoTBからPC版WoTに音声Modを移植できる機能を追加<br>
・"WoTから移植"画面にて、無効なファイルを選択した際、操作不能になる問題を修正<br>
・SEの有無を設定する画面にて、SEを選択し再生する際メモリ使用率が徐々に増える問題を修正<br>
・WAVに変換する際の速度と安定性を向上<br>
・その他細かなバグを修正<br>
<br>
V1.3.1<br>
・PC版WoTから移植できる.bnkファイルを追加<br>
・PC版WoTから移植する画面にて、.bnkファイルを選択していない状態でクリアボタンを押すとクラッシュする問題を修正<br>
・音楽プレイヤーにて、指定した時間内をループできる機能を追加<br>
Shift+Sキーで再生開始位置を指定　Shift+Eキーで再生終了位置を指定 Shift+Cで時間指定を解除<br>
・不要なコードを削除<br>
<br>
V1.3.0変更点<br>
・PC版WoTの音声をWoTB用に変換する機能を追加(めっちゃ大変でした...)<br>
・WwiseでSEを無効化できるように変更<br>
・.bnkファイルを音楽プレイヤーで指定したデバイスで再生するように変更<br>
・音声制作ツールで、特定の操作をするとクラッシュする問題を修正<br>
・音量調整機能にて、ファイル数が多いと機能しなくなる問題を修正<br>
・FSB変換ツールで、既に選択されている状態からFSBファイルを再び選択するとクラッシュする問題を修正<br>
・一部仕様を変更<br>
・その他軽度なバグを修正<br>
<br>
V1.2.9～V1.2.9.9変更点<br>
・ソフトのアップデートなしで対応するFSBの数を増加できるように変更<br>
・実行ファイルを軽量化し、アップデートにかかる時間を大幅に削減<br>
・公開したModを編集するとき、BGMModにチェックを入れると公開できない問題を修正(Thanks to yurina_taki)<br>
・Wwiseのプロジェクトファイルをダウンロードする際、進捗を確認できるように変更<br>
・音楽プレイヤーにて、音程と速度を同期させるオプションを追加<br>
・Mod配布画面にて、再生するサウンドデバイスを、音楽プレイヤーで指定したデバイスで再生するように変更<br>
・音声制作画面にて、音声を追加すると左のリストが未選択になり、その状態で音声を再生しようとするとクラッシュする問題を修正(V1.2.9.7から発生)<br>
・音楽プレイヤーで再生しながら音声制作画面に行くと曲と音声の音量が同期されてしまう問題を修正<br>
・その他軽度なバグを修正<br>
・要望により、音声Mod作成画面にてファイルが選択されていない項目の色を変更してわかりやすくするオプションを追加<br>
・"Wwiseに移植"にて、サポートするFSBの数を増加(中身のファイル名が概ね合っていれば認識するように変更)<br>
・"Wwiseに移植"にて、dvpl化とWoTBに適応するかのオプションを追加<br>
・Shift + Fキーで画面サイズを変更できるように(フルスクリーン・ウィンドウサイズ)<br>
※ウィンドウサイズの場合はマウスをドラッグすることで位置を変更できます。<br>
・音楽プレイヤーにて、動画位置を変更したあとクリックを離しても位置が変更され続ける場合がある問題を修正<br>
・ファイル容量が大きいとdvpl化できなくなる問題を修正(Thanks to yurina_taki!!!)<br>
<br>
V1.2.9変更点<br>
・新サウンドエンジンに対応<br>
・↑に伴い、音声ファイルを.wemに変換->.bnkに適応を自動で行う機能を追加<br>
ただし、上級者向けの画面では音声の差し替えがメインなので音声の追加はできません。<br>
・FmodのFSBファイルをWwiseのBNKファイルに移植してくれる機能を追加(制限あり)<br>
・.bnkファイルの内容を抽出する機能を追加<br>
・.bnkファイル内のサウンドを再生できる機能を追加(ファイルに展開して再生するため時間がかかる場合があります)<br>
・"Mod配布"の仕様を新サウンドエンジン用に変更(依存のModは非公開になっています。対応でき次第追加します)<br>
↑の画面内にある復元機能も更新していますので正常に実行されます。<br>
・BGM作成の仕様も新サウンドエンジン用に変更<br>
・.wavファイルに変換するとき、マルチスレッドで動作するように変更(処理速度大幅UP↑↑↑CPU使用率100%いくけど許してください。)<br>
・音声作成画面の操作にて、選択したファイルを取り消すときに別のファイルが取り消されていた問題を修正<br>
・音楽プレイヤーにて、波形を表示できる機能を追加<br>
・その他軽度なバグを修正<br>
・一部仕様を変更<br>
<br>
V1.2.8変更点<br>
・FSBファイルを抽出できる機能を追加(.aac .flac .mp3 .ogg .wav .webm .wma形式に対応)<br>
・FSBファイル専用の再生プレイヤーを追加<br>
↑はともにWoTB用ツール内のFSB変換ツールに入っています。<br>
・DDSをPNGに、PNGをDDSにエンコードできる機能を追加(WoTB用ツールに入っています。)<br>
.dds(BC1～BC7まで対応) .png .jpg .bmp .gif .tiff .exifを相互変換できます。(WoTBのddsはBC3形式です。)<br>
↑はWoTB用ツール内の画像変換ツールに入っています。<br>
・FSBファイルから抽出してできたファイルをFSB形式に戻す機能を追加<br>
↑はWoTB用ツール内のFSB作成ツールに入っています。(FSBから抽出すると.wfsファイルが生成されるのでそれを指定します。)<br>
・音楽プレイヤー、Mod配布画面にてリストの順番を変更できるように<br>
(音楽プレイヤーでは名前順、拡張子順 / Mod配布画面では名前順、配布順)<br>
・チェックボックスのチェックの有無や音量を保存し起動時に適応されるように変更<br>
・Youtubeのタイトルにファイル名として保存できない文字がある場合、正常に保存されない不具合を修正<br>
・ログをクリアできるように変更(ホーム画面でShift+Lキーを押すとダイアログが表示されます。)<br>
・一部の環境でAndroid用の音声Modが作成されない問題を修正<br>
・すべての環境でWoTBのDVPLを解除できない問題を修正<br>
・.dllの位置を変更(dllフォルダに移動)<br>
・軽度なバグを修正<br>
・一部仕様を変更<br>
<br>
V1.2.7変更点<br>
・WoTB用ツールにてFEVファイルを再生できる機能を追加(対応するFSBファイルがないと動作しません。)<br>
・音楽プレイヤーにてYoutubeから取得できる機能を追加(音声か動画か選択できます。)<br>
・音声Mod作成時音声が見つからない場合クラッシュする問題を修正<br>
・エラーが発生した場合Error_Log.txtに内容を出力するように変更(ログをもとに修正します。)<br>
・その他軽度なバグを修正<br>
<br>
V1.2.6変更点<br>
・Android対応のfsbファイルを作成できる機能を追加("Android用"にチェックを入れる必要があります。)<br>
・Android用のModにSEを追加できるように<br>
・アップデートやModのダウンロードの進捗状況を確認できる機能を追加<br>
・チャットを開いたとき自動で一番下までスクロールするように変更<br>
・音声Modを作成できない可能性がある問題を修正<br>
・一部の環境にて音声の音量が変更されない問題を修正<br>
・音楽プレイヤーにて出力デバイスのバグを修正<br>
。音楽プレイヤーでファイルが見つからない場合クラッシュする問題を修正<br>
・一部デザインを変更<br>
・Fmod_Android_Create.exeの仕様を変更<br>
・その他細かなバグの修正や仕様の変更<br>
<br>
V1.2.5変更点<br>
・配布されている音声Modのsounds.yamlがdvpl化されていなかった場合正しくインストールできない問題を修正(Thanks to meniya)<br>
・Android用にFSB単体を作成できるソフトを追加(一時フォルダのFmod_Android_Createにあります。)<br>
↑の使用方法はGithubのFmod_Android_Createを参照してください。<br>
・細かなバグを修正<br>
・一部仕様を変更<br>
<br>
V1.2.4変更点<br>
・音楽プレイヤーで出力デバイスを指定できるように操作を追加<br>
・一時フォルダの位置を変更していた場合音声をロード、セーブ、作成ができなかった問題を修正<br>
・その他軽度なバグを修正<br>
・一部仕様を変更<br>
<br>
V1.2.3変更点<br>
・ソフト内で作成したBGMModがDVPL化されていた場合BGMModとして公開できなかった問題を修正<br>
・Mod配布者自身が公開したModは編集できるように変更(削除、ファイル追加など)<br>
<br>
V1.2.2変更点<br>
・一部の環境で音声が作成されない問題を修正(Thanks to Yurina_Taki!!!)<br>
・パスに日本語が含まれている場合dvplを解除できない問題を修正<br>
・このソフトのアンインストールをソフト内でできるように(ホーム画面でShift + Escキーを押すとメッセージが出ます)<br>
・一時ファイル(キャッシュファイルを含む)のフォルダ場所を指定できるように変更(ホーム画面でShift + Dキーを押すとメッセージが出ます)<br>
・いるかわかりませんが、音楽プレイヤーでバックグラウンド再生できるように変更<br>
・謎のチャット機能を解放(V1.0から既に存在していましたが細かい調整のため非公開でした)<br>
・細かな調整<br>
<br>
V1.2.1変更点<br>
・軽度なバグを修正<br>
・一部仕様を変更<br>
・不必要なライブラリを削除<br>
<br>
V1.2変更点<br>
・致命的なバグを修正<br>
(戦闘中にクラッシュ、FEV+FSBファイルが作成されない場合があるなどを修正)<br>
・アップデートをソフト内でできるように(サーバーに接続&ログインする必要あり)<br>
<br>
V1.1変更点<br>
・WoTBのインストール場所がSteamのインストール場所と同じだった場合フォルダを取得できない問題を修正(Thanks to yurina_taki)<br>
・サーバー機能を一部開放し、ログインすればだれでもModの配布、ダウンロードを行うことができるように<br>
・サーバー容量は1TBです。不必要なファイルのアップロードはおやめください。<br>
<br>
---using Library---<br>
BASS.ASIO.1.3.1.2<br>
BASS.Native.2.4.12.2<br>
Bass.NetWrapper.2.4.12.5<br>
Costura.Fody.4.1.0<br>
FluentFTP.33.0.3<br>
Fody.6.3.0<br>
Obfuscar.2.2.28<br>
SimpleTCP.1.0.24<br>
K4os.Compression.LZ4.1.2.6<br>
Crc32.NET.1.2.0<br>
System.Buffers.4.5.1<br>
System.Memory.4.5.4<br>
System.Runtime.CompilerServices.Unsafe.5.0.0<br>
Cauldron.FMOD(V1.1から導入)<br>
YoutubeExplode(V1.2.7から導入)<br>
DdsFileTypePlusIO(V1.2.8から導入)<br>
BetterFolderBrowser(V1.2.8から導入)
