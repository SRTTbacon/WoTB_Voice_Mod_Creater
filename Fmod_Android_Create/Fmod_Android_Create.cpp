#include <fsbank.h>
#include <thread>
#include <iostream>
#include <fstream>
#include <string>
#include <vector>

unsigned int G_NumOfCores = std::thread::hardware_concurrency();

#define STR(var)
bool checkFileExistence(const std::string& str)
{
    std::ifstream ifs(str);
    bool Temp = ifs.is_open();
    ifs.close();
    return Temp;
}
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
    if (!checkFileExistence(argv[2]))
    {
        std::cout << "指定したファイルが存在しません。" << std::endl;
        return 0;
    }
    std::ifstream ifs(argv[2]);
    int c;
    while ((c = ifs.get()) >= 0x9);
    if (c != EOF)
    {
        std::cout << "指定したファイルはテキストファイルではありません。" << std::endl;
    }
    ifs.close();
    int Core_Number = 1;
    if (G_NumOfCores != 1)
    {
        Core_Number = G_NumOfCores / 2;
    }
    FSBank_Init(FSBANK_FSBVERSION::FSBANK_FSBVERSION_FSB5, FSBANK_INIT_NORMAL, Core_Number, NULL);
    int Number = 0;

    std::vector<std::string> lines;
    std::string line;
    std::ifstream input_file(argv[2]);
    while (getline(input_file, line))
    {
        lines.push_back(line);
        std::cout << line << std::endl;
    }
    input_file.close();

    FSBANK_SUBSOUND* Sounds = new FSBANK_SUBSOUND();
    for (int i = 0; i < (int)lines.size(); i++) {
        FSBANK_SUBSOUND a;
        const char** Temp_01;
        Temp_01 = new const char*(lines[i].c_str());
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
    FSBANK_RESULT result = FSBank_Build(Sounds, Number, FSBANK_FORMAT::FSBANK_FORMAT_IMAADPCM, FSBANK_BUILD_DEFAULT, 100, NULL, argv[1]);
    if (result != FSBANK_RESULT::FSBANK_OK)
    {
        std::cout << "エラーコード:" << std::to_string(result) << std::endl;
        std::cout << "詳しいエラー内容は\"FSBANK_RESULT\"を参照してください。";
    }
    return 0;
}