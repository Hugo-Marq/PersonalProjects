//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//           INSTITUTO POLITÉCNICO DO CÁVADO E DO AVE
//                          2024/2025
//             ENGENHARIA DE SISTEMAS INFORMÁTICOS
//                    VISÃO POR COMPUTADOR
//						 [  GRUPO 15  ]
//					 20349 - Flávio Costa
//					 24160 - Hugo Marques
//					 26402 - Rafael Ferreira
//					 23036 - Marco Alves
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#include <iostream>
#include <string>
#include <chrono>
#include <opencv2\opencv.hpp>
#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>
#include <opencv2\videoio.hpp>

extern "C" {
#include "vc.h"
#include "constants.h"
#include <math.h>
#include <windows.h>

}

TrackedCoin trackedCoins[MAX_TRACKED_COINS];
int coin_id_counter = 1;

void vc_timer(bool pause = false) {
    static bool running = false;
    static std::chrono::steady_clock::time_point previousTime = std::chrono::steady_clock::now();

    if (!running) {
        running = true;
    }
    else {
        std::chrono::steady_clock::time_point currentTime = std::chrono::steady_clock::now();
        std::chrono::steady_clock::duration elapsedTime = currentTime - previousTime;
        std::chrono::duration<double> time_span = std::chrono::duration_cast<std::chrono::duration<double>>(elapsedTime);
        double nseconds = time_span.count();

        std::cout << "Tempo decorrido: " << nseconds << " segundos" << std::endl;
        if (pause) {
            std::cout << "Pressione qualquer tecla para continuar...\n";
            std::cin.get();
        }
    }
}


int main(void) {
	// Permitir UTF-8 na consola para carateres especiais
    SetConsoleOutputCP(CP_UTF8);

    char videofiles[2][20] = {"video1.mp4", "video2.mp4"};
    int num_files = sizeof(videofiles) / sizeof(videofiles[0]);

    TrackedCoin trackedCoinsVideos[2][MAX_TRACKED_COINS] = { 0 };

	for (int v = 0; v < num_files; v++) {
        // Definição do nome do vídeo a processar
        cv::VideoCapture capture;
        Video video;
        std::string str;
        int key = 0;

        // Verifica se o ficheiro de vídeo existe
        capture.open(videofiles[v]);
        if (!capture.isOpened()) {
            std::cerr << "Erro ao abrir o ficheiro de vídeo!\n";
            return 1;
        }

        // Definição do tamanho do vídeo
        video.width = (int)capture.get(cv::CAP_PROP_FRAME_WIDTH);
        video.height = (int)capture.get(cv::CAP_PROP_FRAME_HEIGHT);

        cv::namedWindow("VC - VIDEO", cv::WINDOW_NORMAL);

        vc_timer(false);
        cv::Mat frame;

        // Definição dos limites HSV para as moedas
        int hsvGold[6] = {
            HSV_GOLD_H_MIN, HSV_GOLD_H_MAX, HSV_GOLD_S_MIN,
            HSV_GOLD_S_MAX, HSV_GOLD_V_MIN, HSV_GOLD_V_MAX
        };
        int hsvSilver[6] = {
            HSV_SILVER_H_MIN, HSV_SILVER_H_MAX, HSV_SILVER_S_MIN,
            HSV_SILVER_S_MAX, HSV_SILVER_V_MIN, HSV_SILVER_V_MAX
        };
        int hsvCopper[6] = {
            HSV_COPPER_H_MIN, HSV_COPPER_H_MAX, HSV_COPPER_S_MIN,
            HSV_COPPER_S_MAX, HSV_COPPER_V_MIN, HSV_COPPER_V_MAX
        };

        // Definição dos limites de área para as moedas
        int coinAreas[12] = {
            AREA_2EURO_MIN, AREA_2EURO_MAX,
            AREA_50CENT_MIN, AREA_50CENT_MAX,
            AREA_1EURO_20CENT_5CENT_MIN, AREA_1EURO_20CENT_5CENT_MAX,
            AREA_10CENT_MIN, AREA_10CENT_MAX,
            AREA_2CENT_MIN, AREA_2CENT_MAX,
            AREA_1CENT_MIN, AREA_1CENT_MAX
        };

        // Alocar memória para as imagens
        IVC* image_bgr = vc_image_new(video.width, video.height, 3, 255);
        IVC* image_hsv = vc_image_new(image_bgr->width, image_bgr->height, image_bgr->channels, image_bgr->levels);
        IVC* image_thresh = vc_image_new(image_bgr->width, image_bgr->height, 1, 255);
        IVC* image_binary_blob_labelling = vc_image_new(image_bgr->width, image_bgr->height, 1, 255);

        // Mecanismo de rastreamento de moedas
        for (int i = 0; i < MAX_TRACKED_COINS; i++) {
            trackedCoins[i].active = 0;
        }
        int unique_ids_seen[MAX_TRACKED_COINS] = { 0 };
        int total_unique_coins = 0;
        float total_value = 0.0f;

        // Loop para identificação das moedas
        while (key != 'q') {
            // Captura de um frame do vídeo
            capture.read(frame);
            if (frame.empty()) break;

            // Copia os dados do frame para a imagem BGR
            memcpy(image_bgr->data, frame.data, video.width * video.height * 3);

            // Converte a imagem BGR para HSV
            vc_bgr_to_hsv(image_bgr, image_hsv);

            // Aplica a segmentação HSV para identificar moedas de ouro, prata e cobre
            vc_hsv_to_segmentation_GoldSilverCopper(image_hsv, image_thresh, hsvGold, hsvSilver, hsvCopper);

            // Cria uma instancia Mat com os dados da imagem binária para aplicar morfologia
            cv::Mat src(image_thresh->height, image_thresh->width, CV_8UC1, image_thresh->data);
            cv::morphologyEx(src, src, cv::MORPH_CLOSE, cv::getStructuringElement(cv::MORPH_ELLIPSE, {13, 13}));

            // Variável para contagem de blobs
            int nBlobs;

            // Aplica a rotulagem de blobs binários
            OVC* blobs = vc_binary_blob_labelling(image_thresh, image_binary_blob_labelling, &nBlobs);

            // Validação se blobs são moedas
            for (int i = 0; i < nBlobs; i++) {

                // Verifica se o blob é circular, se está dentro dos limites de área e se está no intervalo de razão de aspecto
                if (!isValidCircularBlob(blobs[i], 0.9f, 0.9f, 1.1f, 10000, 30000)) continue;

                // Determinar o centro do blob
                int cx = blobs[i].x + blobs[i].width / 2;
                int cy = blobs[i].y + blobs[i].height / 2;

                // Flag para verificar se a moeda foi rastreada
                int matched = 0;

                // Verifica se a moeda já foi rastreada
                for (int j = 0; j < MAX_TRACKED_COINS; j++) {
                    if (!trackedCoins[j].active) continue;

                    // Verifica a distância entre a moeda atual e a moeda rastreada
                    float d = distance(trackedCoins[j].cx, trackedCoins[j].cy, cx, cy);

                    // Se a distância for menor que 120 pixels, atualiza as informações da moeda rastreada e atualiza flag matched
                    if (d < 120) {
                        UpdateCoinFlags(&trackedCoins[j], cx, cy);
                        matched = 1;

                        // Prepara o texto com o Valor, o Perímetro e a Área da moeda rastreada a aparecer na moeda
                        char label[130];
                        sprintf_s(label, "V: %.2f, P: %d, A: %d", trackedCoins[j].value, trackedCoins[j].perimeter, trackedCoins[j].area);
                        int label_x = blobs[i].x;
                        int label_y = blobs[i].y - 10;

                        // Previne que o texto saia da tela enquanto exibe a moeda
                        if (label_y < 10) label_y = blobs[i].y + 20;

                        // Mostra o texto na moeda
                        cv::putText(frame, label, cv::Point(label_x, label_y), cv::FONT_HERSHEY_SIMPLEX, 0.4, cv::Scalar(255, 20, 20), 1);
                        break;
                    }
                }

                // Se a moeda não foi rastreada, tenta adicionar uma nova moeda
                if (!matched) {
                    TryAddNewCoin(trackedCoins, &coin_id_counter, cx, cy, blobs[i], unique_ids_seen,
                        &total_unique_coins, &total_value, *image_hsv, hsvGold, hsvSilver, hsvCopper, coinAreas);

                    SaveUniqueTrackedCoins(trackedCoins, trackedCoinsVideos[v], MAX_TRACKED_COINS);

                }

                // Desenha uma caixa delimitadora à volta da moeda na tela
                draw_rectangle(frame.data, video.width, video.height, blobs, i);
            }

            // Verifica a "Idade" da moeda rastreada e remove-a se não for vista por 7 frames
            UpdateTrackedCoinStatus(trackedCoins, MAX_TRACKED_COINS, MAX_MISSING_FRAMES);

            // Informação do total de moedas e total do valor acumulado ao longo do Video
            char text1[40], text2[40], text3[150], text4[150];
            sprintf_s(text1, "Total de moedas: %d", total_unique_coins);
            sprintf_s(text2, "Valor total: %.2f", total_value);
            sprintf_s(text3, "Premir 'P' para pausar o video");
            sprintf_s(text4, "Premir qualquer tecla para \"despausar\"");
            cv::putText(frame, text1, cv::Point(20, 40), cv::FONT_HERSHEY_SIMPLEX, 0.8, cv::Scalar(255, 0, 0), 2);
            cv::putText(frame, text2, cv::Point(20, 80), cv::FONT_HERSHEY_SIMPLEX, 0.8, cv::Scalar(255, 0, 0), 2);
            cv::putText(frame, text3, cv::Point(20, 110), cv::FONT_HERSHEY_SIMPLEX, 0.6, cv::Scalar(255, 0, 0), 1);
            cv::putText(frame, text4, cv::Point(20, 130), cv::FONT_HERSHEY_SIMPLEX, 0.6, cv::Scalar(255, 0, 0), 1);

            // Exibição do vídeo com as moedas rastreadas
            cv::imshow("VC - VIDEO", frame);

            // Verifica se a tecla 'q' foi pressionada para sair do loop
            key = cv::waitKey(1);

			// Se a tecla 'p' for pressionada, pausa o vídeo
            if (key == 'p') {
                cv::waitKey(0);
            }
        }

        // Liberta a memória alocada para as imagens
        vc_image_free(image_bgr);
        vc_image_free(image_hsv);
        vc_image_free(image_thresh);
        vc_image_free(image_binary_blob_labelling);

        // Liberta os recursos do vídeo
        vc_timer(false);
        cv::destroyWindow("VC - VIDEO");
        capture.release();

		// Cria uma imagem para mostrar o resumo, reutilizando a frame
        frame = cv::Mat(video.height, video.width, CV_8UC3, cv::Scalar(220, 220, 220));

        int x_offset = 30;
        int y_offset = 30;

        for (int v = 0; v < 2; v++) {
			// Cabeçalho para cada vídeo
            char header[50];
            sprintf_s(header, "=== VIDEO %d ===", v + 1);
            cv::putText(frame, header, cv::Point(x_offset - 20, y_offset), cv::FONT_HERSHEY_SIMPLEX, 0.6, cv::Scalar(0, 0, 0), 2);
            y_offset += 30;

			// Informação de cada moeda rastreada
            int coin_count = 0;
			float total_value = 0.0f;
            for (int i = 0; i < MAX_TRACKED_COINS; i++) {
                if (trackedCoinsVideos[v][i].active) {
                    const TrackedCoin& coin = trackedCoinsVideos[v][i];
                    char info[150];
                    sprintf_s(info, "ID: %d | Value: %.2f | Area: %d | Perimeter: %d",
                        coin.id, coin.value, coin.area, coin.perimeter);
                    cv::putText(frame, info, cv::Point(x_offset, y_offset), cv::FONT_HERSHEY_SIMPLEX, 0.4, cv::Scalar(30, 30, 30), 1);
                    y_offset += 22;
                    coin_count++;
					total_value += coin.value;
                }
            }

			// Adiciona informação total de moedas e valor
            char totalCoinsInfo[100];
            sprintf_s(totalCoinsInfo, "Total Coins: %d, Total Value : %.2fEuros", coin_count, total_value);
            y_offset += 10;
            cv::putText(frame, totalCoinsInfo, cv::Point(x_offset, y_offset), cv::FONT_HERSHEY_SIMPLEX, 0.45, cv::Scalar(20, 20, 20), 1);

			// Reposiciona os offsets para o próximo vídeo
            y_offset = 30;
            x_offset += 400;
        }

		// Mostra mensagem de prompt para continuar
        char info2[150];
        sprintf_s(info2, "Prima qualquer tecla para continuar...");
        cv::putText(frame, info2, cv::Point(30, frame.rows - 30), cv::FONT_HERSHEY_SIMPLEX, 0.5, cv::Scalar(30, 30, 30), 1);


		// Mostra sumário dos vídeos
        cv::imshow("VC - VIDEO", frame);
        std::cout << "\nResumo de moedas dos dois vídeos.\nPressione qualquer tecla para sair...\n";
        cv::waitKey(0);

	}
    return 0;
}

