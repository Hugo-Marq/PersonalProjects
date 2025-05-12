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
#ifndef VC_H 
#define VC_H

#include <stdio.h> 
#include <stdlib.h>
#define VC_DEBUG

/**
 * Estrutura de Dados para imagem.
 */
typedef struct {
	unsigned char *data;
	int width, height;
	int channels;
	int levels;
	int bytesperline;
} IVC;

/**
 * Estrutura de Dados para blobs.
 */
typedef struct {
	int x, y, width, height;
	int area;
	int xc, yc;
	int perimeter;
	int label;
} OVC;

/**
 * Estrutura de Dados para Moedas Rastreadas.
 */
typedef struct {
	int id;
	int cx, cy;
	int frames_seen;
	int frames_missing;
	int active;
	int perimeter;
	int area;
	float value;
} TrackedCoin;

/**
 * Estrutura de Dados para Video.
 */
typedef struct {
	int width, height;
	int ntotalframes;
	int fps;
	int nframe;
} Video;


#pragma region I/O Functions

/**
 * Aloca memória para uma nova imagem.
 *
 * \param width - largura da imagem
 * \param height - altura da imagem
 * \param channels - número de canais (1 = grayscale, 3 = RGB)
 * \param levels - número de níveis (2 = binário, 256 = grayscale)
 * \return - ponteiro para a nova imagem ou NULL em caso de erro
 */
IVC *vc_image_new(int width, int height, int channels, int levels);

/**
 * Liberta a memória alocada para uma imagem.
 *
 * \param image - ponteiro para a imagem a libertar
 * \return - ponteiro NULL
 */
IVC *vc_image_free(IVC *image);

/**
 * Lê uma imagem de um arquivo no formato PBM, PGM ou PPM.
 *
 * \param filename - nome do arquivo a ler
 * \return - ponteiro para a imagem lida ou NULL em caso de erro
 */
IVC *vc_read_image(char *filename);

/**
 * Escreve uma imagem em um arquivo no formato PBM, PGM ou PPM.
 *
 * \param filename - nome do arquivo a escrever
 * \param image - ponteiro para a imagem a escrever
 * \return - 1 em caso de sucesso, 0 em caso de erro
 */
int vc_write_image(char *filename, IVC *image);

#pragma endregion

#pragma region Tp_Functions

/**
 * Aplica segmentação a uma imagem de HSV para identificar objetos de ouro, prata e cobre.
 *
 * \param src - imagem de entrada em BGR
 * \param dst - imagem de saída em HSV
 * \param gold - array com os limites de HSV para o ouro
 * \param silver - array com os limites de HSV para a prata
 * \param copper - array com os limites de HSV para o cobre
 * \return - 1 se a conversão e segmentação foram bem-sucedidas, 0 caso contrário
 */
int vc_hsv_to_segmentation_GoldSilverCopper(IVC* src, IVC* dst, int* hsvGold, int* silver, int* copper);

/**
 * Converte uma imagem BGR (Blue, Green, Red) para HSV (Hue, Saturation, Value).
 *
 * \param src - ponteiro para a imagem de origem (BGR)
 * \param dst - ponteiro para a imagem de destino (HSV)
 * \return - 1 em caso de sucesso, 0 em caso de erro
 */
int vc_bgr_to_hsv(IVC* src, IVC* dst);

/**
 * Realiza a rotulagem de blobs binários em uma imagem.
 *
 * \param src - imagem de entrada (binária)
 * \param dst - imagem de saída (rotulada)
 * \param nlabels - ponteiro para armazenar o número de rótulos encontrados
 * \return - ponteiro para a lista de blobs rotulados
 */
OVC* vc_binary_blob_labelling(IVC* src, IVC* dst, int* nlabels);

/**
 * Desenha um retângulo em uma imagem, representando um blob rotulado.
 *
 * \param frame - ponteiro para a imagem onde o retângulo será desenhado
 * \param width - largura da imagem
 * \param height - altura da imagem
 * \param blobs - ponteiro para a lista de blobs rotulados
 * \param i - índice do blob a ser desenhado
 */
void draw_rectangle(unsigned char* frame, int width, int height, OVC* blobs, int i);

/**
 * Calcula a distância entre dois pontos (x1, y1) e (x2, y2).
 *
 * \param x1 - coordenada x do primeiro ponto
 * \param y1 - coordenada y do primeiro ponto
 * \param x2 - coordenada x do segundo ponto
 * \param y2 - coordenada y do segundo ponto
 * \return - distância entre os dois pontos
 */
float distance(int x1, int y1, int x2, int y2);

/**
 * Verifica se um blob é circular com base em sua circularidade, razão de aspecto e área.
 *
 * \param blob - estrutura OVC representando o blob
 * \param minCircularity - valor mínimo de circularidade
 * \param minAspectRatio - razão de aspecto mínima
 * \param MaxAspectRacio - razão de aspecto máxima
 * \param minArea - área mínima
 * \param maxArea - área máxima
 * \return - 1 se o blob for circular, 0 caso contrário
 */
bool isValidCircularBlob(OVC blob, float minCircularity, float minAspectRatio, float MaxAspectRacio, int minArea, int maxArea);

/**
 * Calcula o valor de uma moeda com base em sua área e metal.
 *
 * \param blob - estrutura OVC representando a moeda
 * \param hsv_image - imagem HSV da moeda
 * \param gold - array com os limites de HSV para o ouro
 * \param silver - array com os limites de HSV para a prata
 * \param copper - array com os limites de HSV para o cobre
 * \param areas - array com os limites de área para diferentes moedas
 * \param id - ID do blob
 * \return - valor da moeda (em euros)
 */
float GetCoinValue(OVC blob, IVC* hsv_image, int* gold, int* silver, int* copper, int* areas, int id);

/**
 * Exibe informações sobre uma moeda rastreada.
 *
 * \param coin - estrutura TrackedCoin representando a moeda
 */
void DisplayCoinInfo(TrackedCoin coin);

/**
 * Atualiza as informações de uma moeda rastreada.
 *
 * \param coin - ponteiro para a estrutura TrackedCoin representando a moeda
 * \param cx - coordenada x do centro da moeda
 * \param cy - coordenada y do centro da moeda
 */
void UpdateCoinFlags(TrackedCoin* coin, int cx, int cy);

/**
 * Tenta adicionar uma nova moeda à lista de moedas rastreadas.
 *
 * \param trackedCoins - ponteiro para a lista de moedas rastreadas
 * \param coin_id_counter - ponteiro para o contador de IDs de moedas
 * \param cx - coordenada x do centro da moeda
 * \param cy - coordenada y do centro da moeda
 * \param blob - estrutura OVC representando a moeda
 * \param unique_ids_seen - ponteiro para a lista de IDs únicos vistos
 * \param total_unique_coins - ponteiro para o número total de moedas únicas
 * \param total_value - ponteiro para o valor total das moedas
 * \param image_hsv - imagem HSV da moeda
 * \param hsvGold - array com os limites de HSV para o ouro
 * \param hsvSilver - array com os limites de HSV para a prata
 * \param hsvCopper - array com os limites de HSV para o cobre
 * \param coinAreas - array com os limites de área para diferentes moedas
 */
void TryAddNewCoin(TrackedCoin* trackedCoins, int* coin_id_counter, int cx, int cy, OVC blob, int* unique_ids_seen,
	int* total_unique_coins, float* total_value, IVC image_hsv, int* hsvGold, int* hsvSilver, int* hsvCopper, int* coinAreas);

/**
 * Atualiza o status das moedas rastreadas, removendo as que não foram vistas por um certo número de frames.
 *
 * \param trackedCoins - ponteiro para a lista de moedas rastreadas
 * \param max_tracked_coins - número máximo de moedas rastreadas
 * \param max_missing_frames - número máximo de frames sem verificação
 */
void UpdateTrackedCoinStatus(TrackedCoin* trackedCoins, int max_tracked_coins, int max_missing_frames);

void SaveUniqueTrackedCoins(TrackedCoin* source, TrackedCoin* destination, int maxCoins);
#pragma endregion
#endif // VC_H