//#include <iostream>
//#include <string>
//#include <chrono>
//#include <opencv2\opencv.hpp>
//#include <opencv2\core.hpp>
//#include <opencv2\highgui.hpp>
//#include <opencv2\videoio.hpp>
//
//extern "C" {
//#include "vc.h"
//#include <math.h>
//}
//
//#define MAX_BLOBS 1000
//#define MAX_TRACKED_COINS 100
//
//void vc_timer(void) {
//	static bool running = false;
//	static std::chrono::steady_clock::time_point previousTime = std::chrono::steady_clock::now();
//
//	if (!running) {
//		running = true;
//	}
//	else {
//		std::chrono::steady_clock::time_point currentTime = std::chrono::steady_clock::now();
//		std::chrono::steady_clock::duration elapsedTime = currentTime - previousTime;
//
//		// Tempo em segundos.
//		std::chrono::duration<double> time_span = std::chrono::duration_cast<std::chrono::duration<double>>(elapsedTime);
//		double nseconds = time_span.count();
//
//		std::cout << "Tempo decorrido: " << nseconds << "segundos" << std::endl;
//		std::cout << "Pressione qualquer tecla para continuar...\n";
//		std::cin.get();
//	}
//}
//
//
//int main(void) {
//	// Vídeo
//	char videofile[20] = "video2.mp4";
//	cv::VideoCapture capture;
//	struct
//	{
//		int width, height;
//		int ntotalframes;
//		int fps;
//		int nframe;
//	} video;
//	// Outros
//	std::string str;
//	int key = 0;
//
//	capture.open(videofile);
//
//	//Verificar se video foi aberto
//	if (!capture.isOpened())
//	{
//		std::cerr << "Erro ao abrir o ficheiro de v�deo!\n";
//		return 1;
//	}
//
//	//Numero de frames, fps e resolução do vídeo
//	video.ntotalframes = (int)capture.get(cv::CAP_PROP_FRAME_COUNT);
//	video.fps = (int)capture.get(cv::CAP_PROP_FPS);
//	video.width = (int)capture.get(cv::CAP_PROP_FRAME_WIDTH);
//	video.height = (int)capture.get(cv::CAP_PROP_FRAME_HEIGHT);
//
//	//janela para apresentar o vídeo
//	cv::namedWindow("VC - VIDEO", cv::WINDOW_NORMAL);
//	cv::namedWindow("VC - VIDEO2", cv::WINDOW_NORMAL);
//
//	// Resize the window manually if needed
//	cv::resizeWindow("VC - VIDEO", 640, 480);
//	cv::resizeWindow("VC - VIDEO2", 640, 480);
//
//	//Iniciar timer
//	vc_timer();
//	
//	cv::Mat frame, dst;
//
//	
//	//{h_min, h_max, s_min, s_max, v_min, v_max }
//	int hsvGold[6] = { 29, 90, 20, 100, 12, 60 };
//	int hsvSilver[6] = { 48, 180, 3, 23, 14, 41 };
//	int hsvCopper[6] = { 18, 40, 40, 80, 15, 45 };
//
//
//	// Criar imagens IVC
//	IVC* original_bgr = vc_image_new(video.width, video.height, 3, 255);
//	IVC* image_bgr = vc_image_new(original_bgr->width, original_bgr->height, original_bgr->channels, original_bgr->levels);
//	IVC* image_hsv = vc_image_new(original_bgr->width, original_bgr->height, original_bgr->channels, original_bgr->levels);
//	IVC* image_thresh = vc_image_new(original_bgr->width, original_bgr->height, 1, 255);
//	IVC* image_binary_blob_labelling = vc_image_new(original_bgr->width, original_bgr->height, 1, 255);
//
//	TrackedCoin trackedCoins[MAX_TRACKED_COINS];
//	int coin_id_counter = 1;
//
//	while (key != 'q') {
//		/* Leitura de uma frame do v�deo */
//		capture.read(frame);
//
//		/* Verifica se conseguiu ler a frame */
//		if (frame.empty()) break;
//
//		/* N�mero da frame a processar */
//		video.nframe = (int)capture.get(cv::CAP_PROP_POS_FRAMES);
//
//		// Copiar dados de imagem da estrutura cv::Mat para IVC
//		memcpy(original_bgr->data, frame.data, video.width* video.height * 3);
//		memcpy(image_bgr->data, frame.data, video.width* video.height * 3);
//
//		//Converte de BGR para HSV
//		vc_bgr_to_hsv(image_bgr, image_hsv);
//
//		vc_hsv_to_segmentation_GoldSilverCopper(image_hsv, image_thresh, hsvGold, hsvSilver, hsvCopper);
//
//		cv::Mat src(image_thresh->height, image_thresh->width, CV_8UC1, image_thresh->data);
//		cv::Mat kernel = cv::getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(15, 15));
//		cv::morphologyEx(src, dst, cv::MORPH_CLOSE, kernel);
//
//		memcpy(image_thresh->data, dst.data, video.width* video.height * 1);
//
//		int nBlobs;
//		OVC* blobs = vc_binary_blob_labelling(image_thresh, image_binary_blob_labelling, &nBlobs);
//
//		cv::Mat labeledFrame(video.height, video.width, CV_8UC3, image_bgr->data);
//		// Filter and draw bounding boxes on circular blobs (coins)
//		for (int i = 0; i < nBlobs; i++) {
//			// Defensive: avoid division by zero
//			if (blobs[i].perimeter <= 0) continue;
//
//			// Calculate circularity
//			float circularity = (4.0f * 3.14159265f * blobs[i].area) / (blobs[i].perimeter * blobs[i].perimeter);
//
//			// Update: Increase circularity threshold to 0.8 for stricter detection
//			if (circularity >= 0.9f) {
//				// Check aspect ratio (coins should have a ratio close to 1)
//				float aspect_ratio = (float)blobs[i].width / blobs[i].height;
//				if (aspect_ratio > 1.1f || aspect_ratio < 0.9f) continue;
//
//				// Filter by size (coins typically fall within a size range)
//				if (blobs[i].area < 10000 || blobs[i].area > 30000) continue;
//
//				// Draw rectangle on color image (image_bgr)
//				/*draw_rectangle(image_bgr, blobs[i].x, blobs[i].y, blobs[i].width, blobs[i].height, 0, 0, 255);*/
//				cv::rectangle(
//					labeledFrame,
//					cv::Rect(blobs[i].x, blobs[i].y, blobs[i].width, blobs[i].height),
//					cv::Scalar(0, 0, 255), // Red color
//					2                     // Thickness
//				);
//
//				// Draw label above the rectangle
//				std::string label = "Coin";
//				cv::putText(
//					labeledFrame,
//					label,
//					cv::Point(blobs[i].x, blobs[i].y - 10),
//					cv::FONT_HERSHEY_SIMPLEX,
//					0.7,                   // Font scale
//					cv::Scalar(0, 0, 0), // Green color
//					2                     // Thickness
//				);
//			}
//		}
//
//		///* Exemplo de inser��o texto na frame */
//		//str = std::string("RESOLUCAO: ").append(std::to_string(video.width)).append("x").append(std::to_string(video.height));
//		//cv::putText(frame, str, cv::Point(20, 25), cv::FONT_HERSHEY_SIMPLEX, 1.0, cv::Scalar(0, 0, 0), 2);
//		//cv::putText(frame, str, cv::Point(20, 25), cv::FONT_HERSHEY_SIMPLEX, 1.0, cv::Scalar(255, 255, 255), 1);
//
//		cv::Mat binaryImageMat(video.height, video.width, CV_8UC1, image_thresh->data);
//		cv::Mat coloredWithRectangles(video.height, video.width, CV_8UC3, image_bgr->data);
//		cv::imshow("VC - VIDEO", coloredWithRectangles);
//		cv::imshow("VC - VIDEO2", binaryImageMat);
//
//		/* Sai da aplica��o, se o utilizador premir a tecla 'q' */
//		key = cv::waitKey(20);
//	}
//
//	vc_image_free(original_bgr);
//	vc_image_free(image_bgr);
//	vc_image_free(image_hsv);
//	vc_image_free(image_thresh);
//	vc_image_free(image_binary_blob_labelling);
//
//	/* Para o timer e exibe o tempo decorrido */
//	vc_timer();
//
//	/* Fecha a janela */
//	cv::destroyWindow("VC - VIDEO2");
//	cv::destroyWindow("VC - VIDEO");
//
//	/* Fecha o ficheiro de v�deo */
//	capture.release();
//
//	return 0;
//}

#include <iostream>
#include <string>
#include <chrono>
#include <opencv2\opencv.hpp>
#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>
#include <opencv2\videoio.hpp>

extern "C" {
#include "vc.h"
#include <math.h>
}

#define MAX_BLOBS 1000
#define MAX_TRACKED_COINS 100

// === TRACKING CODE START ===
typedef struct {
    int id;
    int cx, cy;
    int frames_seen;
    int frames_missing;
    int active;
} TrackedCoin;

TrackedCoin trackedCoins[MAX_TRACKED_COINS];
int coin_id_counter = 1;

float distance(int x1, int y1, int x2, int y2) {
    return sqrtf((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}
// === TRACKING CODE END ===

void vc_timer(void) {
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
        std::cout << "Pressione qualquer tecla para continuar...\n";
        std::cin.get();
    }
}

int main(void) {
    char videofile[20] = "video1.mp4";
    cv::VideoCapture capture;
    struct {
        int width, height;
        int ntotalframes;
        int fps;
        int nframe;
    } video;
    std::string str;
    int key = 0;

    capture.open(videofile);

    if (!capture.isOpened()) {
        std::cerr << "Erro ao abrir o ficheiro de vídeo!\n";
        return 1;
    }

    video.ntotalframes = (int)capture.get(cv::CAP_PROP_FRAME_COUNT);
    video.fps = (int)capture.get(cv::CAP_PROP_FPS);
    video.width = (int)capture.get(cv::CAP_PROP_FRAME_WIDTH);
    video.height = (int)capture.get(cv::CAP_PROP_FRAME_HEIGHT);

    cv::namedWindow("VC - VIDEO", cv::WINDOW_NORMAL);
    cv::namedWindow("VC - VIDEO2", cv::WINDOW_NORMAL);
    cv::resizeWindow("VC - VIDEO", 640, 480);
    cv::resizeWindow("VC - VIDEO2", 640, 480);

    vc_timer();

    cv::Mat frame, dst;

    int hsvGold[6] = { 29, 90, 20, 100, 12, 60 };
    int hsvSilver[6] = { 45, 180, 3, 23, 13, 41 };
    int hsvCopper[6] = { 18, 40, 40, 80, 15, 45 };

    IVC* original_bgr = vc_image_new(video.width, video.height, 3, 255);
    IVC* image_bgr = vc_image_new(original_bgr->width, original_bgr->height, original_bgr->channels, original_bgr->levels);
    IVC* image_hsv = vc_image_new(original_bgr->width, original_bgr->height, original_bgr->channels, original_bgr->levels);
    IVC* image_thresh = vc_image_new(original_bgr->width, original_bgr->height, 1, 255);
    IVC* image_binary_blob_labelling = vc_image_new(original_bgr->width, original_bgr->height, 1, 255);

    // === TRACKING CODE START ===
    for (int i = 0; i < MAX_TRACKED_COINS; i++) {
        trackedCoins[i].active = 0;
    }
    int unique_ids_seen[MAX_TRACKED_COINS] = { 0 };
    int total_unique_coins = 0;
    // === TRACKING CODE END ===

    while (key != 'q') {
        capture.read(frame);
        if (frame.empty()) break;
        video.nframe = (int)capture.get(cv::CAP_PROP_POS_FRAMES);

        memcpy(original_bgr->data, frame.data, video.width * video.height * 3);
        memcpy(image_bgr->data, frame.data, video.width * video.height * 3);

        vc_bgr_to_hsv(image_bgr, image_hsv);
        vc_hsv_to_segmentation_GoldSilverCopper(image_hsv, image_thresh, hsvGold, hsvSilver, hsvCopper);

        cv::Mat src(image_thresh->height, image_thresh->width, CV_8UC1, image_thresh->data);// usar memcpy
        cv::Mat kernel = cv::getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(13, 13));
        cv::morphologyEx(src, dst, cv::MORPH_CLOSE, kernel);
        memcpy(image_thresh->data, dst.data, video.width * video.height * 1);

        int nBlobs;
        OVC* blobs = vc_binary_blob_labelling(image_thresh, image_binary_blob_labelling, &nBlobs);

        cv::Mat labeledFrame(video.height, video.width, CV_8UC3, image_bgr->data);

        for (int i = 0; i < nBlobs; i++) {
            if (blobs[i].perimeter <= 0) continue;
            float circularity = (4.0f * 3.14159265f * blobs[i].area) / (blobs[i].perimeter * blobs[i].perimeter);
            if (circularity >= 0.9f) {
                float aspect_ratio = (float)blobs[i].width / blobs[i].height;
                if (aspect_ratio > 1.1f || aspect_ratio < 0.9f) continue;
                if (blobs[i].area < 10000 || blobs[i].area > 30000) continue;

                int cx = blobs[i].x + blobs[i].width / 2;
                int cy = blobs[i].y + blobs[i].height / 2;

                int matched = 0;
                for (int j = 0; j < MAX_TRACKED_COINS; j++) {
                    if (!trackedCoins[j].active) continue;
                    float d = distance(trackedCoins[j].cx, trackedCoins[j].cy, cx, cy);
                    if (d < 120) {
                        trackedCoins[j].cx = cx;
                        trackedCoins[j].cy = cy;
                        trackedCoins[j].frames_seen++;
                        trackedCoins[j].frames_missing = 0;
                        matched = 1;

                        char label[10];
                        sprintf_s(label, "ID %d", trackedCoins[j].id);
                        cv::putText(labeledFrame, label, cv::Point(cx, cy), cv::FONT_HERSHEY_SIMPLEX, 0.6, cv::Scalar(0, 255, 255), 2);
                        break;
                    }
                }

                if (!matched) {
                    for (int j = 0; j < MAX_TRACKED_COINS; j++) {
                        if (!trackedCoins[j].active) {
                            trackedCoins[j].id = coin_id_counter++;
                            trackedCoins[j].cx = cx;
                            trackedCoins[j].cy = cy;
                            trackedCoins[j].frames_seen = 1;
                            trackedCoins[j].frames_missing = 0;
                            trackedCoins[j].active = 1;

                            // Register unique ID
                            int id = trackedCoins[j].id;
                            int already_counted = 0;
                            for (int k = 0; k < total_unique_coins; k++) {
                                if (unique_ids_seen[k] == id) {
                                    already_counted = 1;
                                    break;
                                }
                            }
                            if (!already_counted) {
                                unique_ids_seen[total_unique_coins++] = id;
                            }

                            break;
                        }
                    }
                }


                cv::rectangle(labeledFrame, cv::Rect(blobs[i].x, blobs[i].y, blobs[i].width, blobs[i].height), cv::Scalar(0, 0, 255), 2);
            }
        }

        for (int j = 0; j < MAX_TRACKED_COINS; j++) {
            if (trackedCoins[j].active) {
                trackedCoins[j].frames_missing++;
                if (trackedCoins[j].frames_missing > 7) {
                    trackedCoins[j].active = 0;
                }
            }
        }

      

        char text[50];
        sprintf_s(text, "Total Coins: %d", total_unique_coins);

        cv::putText(labeledFrame, text, cv::Point(20, 40), cv::FONT_HERSHEY_SIMPLEX, 1.0, cv::Scalar(0, 0, 0), 2);

        cv::Mat binaryImageMat(video.height, video.width, CV_8UC1, image_thresh->data);
        cv::imshow("VC - VIDEO", labeledFrame);
        cv::imshow("VC - VIDEO2", binaryImageMat);

        key = cv::waitKey(10);
    }

    vc_image_free(original_bgr);
    vc_image_free(image_bgr);
    vc_image_free(image_hsv);
    vc_image_free(image_thresh);
    vc_image_free(image_binary_blob_labelling);

    vc_timer();
    cv::destroyWindow("VC - VIDEO2");
    cv::destroyWindow("VC - VIDEO");
    capture.release();

    return 0;
}

