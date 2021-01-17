#include <fsbank.h>
#include <thread>
#include <iostream>
#include <string>

unsigned int G_NumOfCores = std::thread::hardware_concurrency();

#define STR(var)
int main(int argc, char* argv[])
{
    //引数:出力ファイル場所,フォーマット,*サウンドファイル場所
    if (argc == 0 || argc == 1)
    {
        std::cout << "引数1:保存場所" << std::endl;
        std::cout << "引数2から～:入れるサウンドファイルの場所" << std::endl;
        std::cout << "注意:現在のバージョンではADPCMフォーマットのみ対応しています。" << std::endl;
        return 0;
    }
    int Core_Number = 1;
    if (G_NumOfCores != 1)
    {
        Core_Number = G_NumOfCores / 2;
    }
    FSBank_Init(FSBANK_FSBVERSION::FSBANK_FSBVERSION_FSB5, FSBANK_INIT_NORMAL, Core_Number, NULL);
    int Number = 0;
    FSBANK_SUBSOUND* Sounds = new FSBANK_SUBSOUND();
    for (int i = 2; i < argc; i++) {
        FSBANK_SUBSOUND a;
        const char** Temp_01;
        Temp_01 = new const char*(argv[i]);
        a.fileNames = Temp_01;
        a.numFileNames = 1;
        a.speakerMap = FSBANK_SPEAKERMAP::FSBANK_SPEAKERMAP_DEFAULT;
        a.overrideFlags = FSBANK_BUILD_DEFAULT;
        a.overrideQuality = 100;
        a.desiredSampleRate = 44100.0f;
        a.percentOptimizedRate = 100.0f;
        Sounds[Number] = a;
        Number++;
    }
    FSBANK_RESULT result = FSBank_Build(Sounds, Number, FSBANK_FORMAT::FSBANK_FORMAT_FADPCM, FSBANK_BUILD_DEFAULT, 100, NULL, argv[1]);
    if (result != FSBANK_RESULT::FSBANK_OK)
    {
        std::cout << "エラーコード:" << std::to_string(result) << std::endl;
        std::cout << "詳しいエラー内容は\"FSBANK_RESULT\"を参照してください。";
    }
    return 0;
}