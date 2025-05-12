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

// Desabilita (no MSVC++) warnings de funções não seguras (fopen, sscanf, etc...)
#define _CRT_SECURE_NO_WARNINGS

#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <malloc.h>
#include <math.h>
#include <stdbool.h>
#include "vc.h"
#include "constants.h"

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
IVC* vc_image_new(int width, int height, int channels, int levels)
{
	IVC* image = (IVC*)malloc(sizeof(IVC));

	if (image == NULL) return NULL;
	if ((levels <= 0) || (levels > 256)) return NULL;

	image->width = width;
	image->height = height;
	image->channels = channels;
	image->levels = levels;
	image->bytesperline = image->width * image->channels;
	image->data = (unsigned char*)malloc(image->width * image->height * image->channels * sizeof(char));

	if (image->data == NULL)
	{
		return vc_image_free(image);
	}

	return image;
}

/**
 * Liberta a memória alocada para uma imagem.
 *
 * \param image - ponteiro para a imagem a libertar
 * \return - NULL
 */
IVC* vc_image_free(IVC* image)
{
	if (image != NULL)
	{
		if (image->data != NULL)
		{
			free(image->data);
			image->data = NULL;
		}
		free(image);
		image = NULL;
	}
	return image;
}

/**
 * Lê um token de um arquivo, ignorando comentários e espaços em branco.
 *
 * \param file - ponteiro para o arquivo
 * \param tok - buffer para armazenar o token lido
 * \param len - tamanho máximo do buffer
 * \return - ponteiro para o buffer do token
 */
char* netpbm_get_token(FILE* file, char* tok, int len)
{
	char* t;
	int c;

	for (;;)
	{
		while (isspace(c = getc(file)));
		if (c != '#') break;
		do c = getc(file);
		while ((c != '\n') && (c != EOF));
		if (c == EOF) break;
	}

	t = tok;

	if (c != EOF)
	{
		do
		{
			*t++ = c;
			c = getc(file);
		} while ((!isspace(c)) && (c != '#') && (c != EOF) && (t - tok < len - 1));

		if (c == '#') ungetc(c, file);
	}

	*t = 0;

	return tok;
}

/**
 * Converte uma imagem binária (unsigned char) para uma imagem em bits (unsigned char).
 *
 * \param datauchar - ponteiro para os dados da imagem binária
 * \param databit - ponteiro para os dados da imagem em bits
 * \param width - largura da imagem
 * \param height - altura da imagem
 * \return - número total de bytes convertidos
 */
long int unsigned_char_to_bit(unsigned char* datauchar, unsigned char* databit, int width, int height)
{
	int x, y;
	int countbits;
	long int pos, counttotalbytes;
	unsigned char* p = databit;

	*p = 0;
	countbits = 1;
	counttotalbytes = 0;

	for (y = 0; y < height; y++)
	{
		for (x = 0; x < width; x++)
		{
			pos = width * y + x;

			if (countbits <= 8)
			{
				// Numa imagem PBM:
				// 1 = Preto
				// 0 = Branco
				//*p |= (datauchar[pos] != 0) << (8 - countbits);

				// Na nossa imagem:
				// 1 = Branco
				// 0 = Preto
				*p |= (datauchar[pos] == 0) << (8 - countbits);

				countbits++;
			}
			if ((countbits > 8) || (x == width - 1))
			{
				p++;
				*p = 0;
				countbits = 1;
				counttotalbytes++;
			}
		}
	}

	return counttotalbytes;
}

/**
 * Converte uma imagem em bits (unsigned char) para uma imagem binária (unsigned char).
 *
 * \param databit - ponteiro para os dados da imagem em bits
 * \param datauchar - ponteiro para os dados da imagem binária
 * \param width - largura da imagem
 * \param height - altura da imagem
 */
void bit_to_unsigned_char(unsigned char* databit, unsigned char* datauchar, int width, int height)
{
	int x, y;
	int countbits;
	long int pos;
	unsigned char* p = databit;

	countbits = 1;

	for (y = 0; y < height; y++)
	{
		for (x = 0; x < width; x++)
		{
			pos = width * y + x;

			if (countbits <= 8)
			{
				// Numa imagem PBM:
				// 1 = Preto
				// 0 = Branco
				//datauchar[pos] = (*p & (1 << (8 - countbits))) ? 1 : 0;

				// Na nossa imagem:
				// 1 = Branco
				// 0 = Preto
				datauchar[pos] = (*p & (1 << (8 - countbits))) ? 0 : 1;

				countbits++;
			}
			if ((countbits > 8) || (x == width - 1))
			{
				p++;
				countbits = 1;
			}
		}
	}
}

/**
 * Lê uma imagem de um arquivo no formato PBM, PGM ou PPM.
 *
 * \param filename - nome do arquivo a ler
 * \return - ponteiro para a imagem lida ou NULL em caso de erro
 */
IVC* vc_read_image(char* filename)
{
	FILE* file = NULL;
	IVC* image = NULL;
	unsigned char* tmp;
	char tok[20];
	long int size, sizeofbinarydata;
	int width, height, channels;
	int levels = 256;
	int v;

	// Abre o ficheiro
	if ((file = fopen(filename, "rb")) != NULL)
	{
		// Efectua a leitura do header
		netpbm_get_token(file, tok, sizeof(tok));

		if (strcmp(tok, "P4") == 0) { channels = 1; levels = 2; }	// Se PBM (Binary [0,1])
		else if (strcmp(tok, "P5") == 0) channels = 1;				// Se PGM (Gray [0,MAX(level,255)])
		else if (strcmp(tok, "P6") == 0) channels = 3;				// Se PPM (RGB [0,MAX(level,255)])
		else
		{
#ifdef VC_DEBUG
			printf("ERROR -> vc_read_image():\n\tFile is not a valid PBM, PGM or PPM file.\n\tBad magic number!\n");
#endif

			fclose(file);
			return NULL;
		}

		if (levels == 2) // PBM
		{
			if (sscanf(netpbm_get_token(file, tok, sizeof(tok)), "%d", &width) != 1 ||
				sscanf(netpbm_get_token(file, tok, sizeof(tok)), "%d", &height) != 1)
			{
#ifdef VC_DEBUG
				printf("ERROR -> vc_read_image():\n\tFile is not a valid PBM file.\n\tBad size!\n");
#endif

				fclose(file);
				return NULL;
			}

			// Aloca memória para imagem
			image = vc_image_new(width, height, channels, levels);
			if (image == NULL) return NULL;

			sizeofbinarydata = (image->width / 8 + ((image->width % 8) ? 1 : 0)) * image->height;
			tmp = (unsigned char*)malloc(sizeofbinarydata);
			if (tmp == NULL) return 0;

#ifdef VC_DEBUG
			printf("\nchannels=%d w=%d h=%d levels=%d\n", image->channels, image->width, image->height, levels);
#endif

			if ((v = fread(tmp, sizeof(unsigned char), sizeofbinarydata, file)) != sizeofbinarydata)
			{
#ifdef VC_DEBUG
				printf("ERROR -> vc_read_image():\n\tPremature EOF on file.\n");
#endif

				vc_image_free(image);
				fclose(file);
				free(tmp);
				return NULL;
			}

			bit_to_unsigned_char(tmp, image->data, image->width, image->height);

			free(tmp);
		}
		else // PGM ou PPM
		{
			if (sscanf(netpbm_get_token(file, tok, sizeof(tok)), "%d", &width) != 1 ||
				sscanf(netpbm_get_token(file, tok, sizeof(tok)), "%d", &height) != 1 ||
				sscanf(netpbm_get_token(file, tok, sizeof(tok)), "%d", &levels) != 1 || levels <= 0 || levels > 256)
			{
#ifdef VC_DEBUG
				printf("ERROR -> vc_read_image():\n\tFile is not a valid PGM or PPM file.\n\tBad size!\n");
#endif

				fclose(file);
				return NULL;
			}

			// Aloca memória para imagem
			image = vc_image_new(width, height, channels, levels + 1);
			if (image == NULL) return NULL;

#ifdef VC_DEBUG
			printf("\nchannels=%d w=%d h=%d levels=%d\n", image->channels, image->width, image->height, levels);
#endif

			size = image->width * image->height * image->channels;

			if ((v = fread(image->data, sizeof(unsigned char), size, file)) != size)
			{
#ifdef VC_DEBUG
				printf("ERROR -> vc_read_image():\n\tPremature EOF on file.\n");
#endif

				vc_image_free(image);
				fclose(file);
				return NULL;
			}
		}

		fclose(file);
	}
	else
	{
#ifdef VC_DEBUG
		printf("ERROR -> vc_read_image():\n\tFile not found.\n");
#endif
	}

	return image;
}

/**
 * Escreve uma imagem em um arquivo no formato PBM, PGM ou PPM.
 *
 * \param filename - nome do arquivo a escrever
 * \param image - ponteiro para a imagem a escrever
 * \return - 1 em caso de sucesso, 0 em caso de erro
 */
int vc_write_image(char* filename, IVC* image)
{
	FILE* file = NULL;
	unsigned char* tmp;
	long int totalbytes, sizeofbinarydata;

	if (image == NULL) return 0;

	if ((file = fopen(filename, "wb")) != NULL)
	{
		if (image->levels == 2)
		{
			sizeofbinarydata = (image->width / 8 + ((image->width % 8) ? 1 : 0)) * image->height + 1;
			tmp = (unsigned char*)malloc(sizeofbinarydata);
			if (tmp == NULL) return 0;

			fprintf(file, "%s \n%d %d\n", "P4", image->width, image->height);

			totalbytes = unsigned_char_to_bit(image->data, tmp, image->width, image->height);
			printf("Total = %ld\n", totalbytes);
			if (fwrite(tmp, sizeof(unsigned char), totalbytes, file) != totalbytes)
			{
#ifdef VC_DEBUG
				fprintf(stderr, "ERROR -> vc_read_image():\n\tError writing PBM, PGM or PPM file.\n");
#endif

				fclose(file);
				free(tmp);
				return 0;
			}

			free(tmp);
		}
		else
		{
			fprintf(file, "%s \n%d %d \n%d\n", (image->channels == 1) ? "P5" : "P6", image->width, image->height, image->levels - 1);

			if (fwrite(image->data, image->bytesperline, image->height, file) != image->height)
			{
#ifdef VC_DEBUG
				fprintf(stderr, "ERROR -> vc_read_image():\n\tError writing PBM, PGM or PPM file.\n");
#endif

				fclose(file);
				return 0;
			}
		}

		fclose(file);

		return 1;
	}

	return 0;
}

#pragma endregion

#pragma region tp_functions

/**
 * Função de comparação para qsort, usada para ordenar os valores de um vetor de unsigned char.
 *
 * \param a - ponteiro para o primeiro elemento
 * \param b - ponteiro para o segundo elemento
 * \return - diferença entre os dois elementos
 */
int compare_uchar(const void* a, const void* b) {
	return (*(unsigned char*)a - *(unsigned char*)b);
}

/**
 * Converte uma imagem BGR (Blue, Green, Red) para HSV (Hue, Saturation, Value).
 *
 * \param src - ponteiro para a imagem de origem (BGR)
 * \param dst - ponteiro para a imagem de destino (HSV)
 * \return - 1 em caso de sucesso, 0 em caso de erro
 */
int vc_bgr_to_hsv(IVC* src, IVC* dst) {

	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int x, y;
	long int pos;
	float rgb_max, rgb_min, red, green, blue, hue, saturation, value;

	// Verifica se as imagens de origem e destino são válidas e têm 3 canais
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 3 || dst->channels != 3) return 0;

	// Percorre cada pixel da imagem
	for (y = 0; y < src->height; y++) {
		for (x = 0; x < src->width; x++) {
			pos = y * src->bytesperline + x * src->channels; // Calcula a posição do pixel atual

			// Extrai os valores de azul, verde e vermelho do pixel BGR
			blue = (float)datasrc[pos];
			green = (float)datasrc[pos + 1];
			red = (float)datasrc[pos + 2];

			// Encontra o valor máximo e mínimo dos canais RGB para determinar o valor (brightness)
			value = rgb_max = (red > green ? (red > blue ? red : blue) : (green > blue ? green : blue));
			rgb_min = (red < green ? (red < blue ? red : blue) : (green < blue ? green : blue));

			// Calcula a saturação e o valor
			if (value == 0.0f) {
				hue = saturation = value; // Se o valor é zero, então a cor é preta
			}
			else {
				// Saturation toma valores entre [0,255]
				hue = saturation = ((rgb_max - rgb_min) / value) * 255.0f; // Saturação é a diferença entre o máximo e mínimo, normalizada

				// Calcula o hue dependendo de qual canal é o máximo
				if (saturation != 0.0f) {
					if (rgb_max == red && green >= blue) {
						hue = 60.0f * (green - blue) / (rgb_max - rgb_min);
					}
					else if (rgb_max == red && blue > green) {
						hue = 360.0f + 60.0f * (green - blue) / (rgb_max - rgb_min);
					}
					else if (rgb_max == green) {
						hue = 120.0f + 60.0f * (blue - red) / (rgb_max - rgb_min);
					}
					else {
						hue = 240.0f + 60.0f * (red - green) / (rgb_max - rgb_min);
					}
				}
			}

			datadst[pos] = (unsigned char)((hue / 360.0f) * 255.0f); // Converte hue para o intervalo [0, 255]
			datadst[pos + 1] = (unsigned char)saturation;            // Saturação já normalizada
			datadst[pos + 2] = (unsigned char)value;                 // Valor (brilho)
		}
	}
	return 1;
}

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
int vc_hsv_to_segmentation_GoldSilverCopper(IVC* src, IVC* dst, int* gold, int* silver, int* copper) {

	unsigned char* datasrc = (unsigned char*)src->data;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	unsigned char* datadst = (unsigned char*)dst->data;

	// Verifica se as imagens de origem e destino são válidas e têm 3 e 1 canais respetivamente
	if ((src->width <= 0) || (src->height <= 0) || (src->data == NULL)) return 0;
	if ((src->width != dst->width) || (src->height != dst->height)) return 0;
	if ((src->channels != 3) || (dst->channels != 1)) return 0;

	for (int y = 0; y < height; y++) {
		for (int x = 0; x < width; x++) {
			int pos = y * bytesperline + x * 3; 

			int h = (int)((float)datasrc[pos] * (360.0f / 255.0f));  // Converter de volta para 0-360
			int s = (int)((float)datasrc[pos + 1] * (100.0f / 255.0f));  // Converter de volta para 0-100
			int v = (int)((float)datasrc[pos + 2] * (100.0f / 255.0f));  // Converter de volta para 0-100

			
			//Verificar se o pixel está dentro dos limites de cor definidos para ouro, prata ou cobre
			if (h >= gold[0] && h <= gold[1] && s >= gold[2] && s <= gold[3] && v >= gold[4] && v <= gold[5]) {
				datadst[y * width + x] = 255; // Foreground (Branco)
			}
			else if (h >= silver[0] && h <= silver[1] && s >= silver[2] && s <= silver[3] && v >= silver[4] && v <= silver[5]) {
				datadst[y * width + x] = 255; // Foreground (Branco)
			}
			else if (h >= copper[0] && h <= copper[1] && s >= copper[2] && s <= copper[3] && v >= copper[4] && v <= copper[5]) {
				datadst[y * width + x] = 255; // Foreground (Branco)
			}
			else{
				datadst[y * width + x] = 0;   // Background (Preto)
			}
		}
	}
	return 1;
}

/**
 * Realiza a rotulagem de blobs binários em uma imagem.
 *
 * \param src - imagem de entrada (binária)
 * \param dst - imagem de saída (rotulada)
 * \param nlabels - ponteiro para armazenar o número de rótulos encontrados
 * \return - ponteiro para a lista de blobs rotulados
 */
OVC* vc_binary_blob_labelling(IVC* src, IVC* dst, int* nlabels) {
	unsigned char* datasrc = src->data;
	unsigned char* datadst = dst->data;
	int width = src->width, height = src->height, channels = src->channels;
	int bytesperline = width * channels;
	int size = bytesperline * height;
	int label = 1, i, x, y, pos;

	// Verifica se as imagens de origem e destino são válidas e têm 1 canal
	if (!src || !dst || !datasrc || !datadst || width <= 0 || height <= 0 || channels != 1 ||
		src->width != dst->width || src->height != dst->height || src->channels != dst->channels) return NULL;

	// Aloca memória para a tabela de rótulos e inicializa
	int maxlabels = width * height;
	int* labeltable = (int*)malloc(sizeof(int) * maxlabels);
	int* newlabels = (int*)calloc(maxlabels, sizeof(int));
	if (!labeltable || !newlabels) return NULL;

	// Inicializa a imagem de saída como uma imagem binária
	for (i = 0; i < size; i++) datadst[i] = (datasrc[i] != 0) ? 255 : 0;

	// Limpa as bordas da imagem de saída
	for (x = 0; x < width; x++) {
		datadst[x] = 0;
		datadst[(height - 1) * bytesperline + x] = 0;
	}
	for (y = 0; y < height; y++) {
		datadst[y * bytesperline] = 0;
		datadst[y * bytesperline + (width - 1)] = 0;
	}

	// Inicializa a tabela de rótulos
	for (i = 0; i < maxlabels; i++) labeltable[i] = i;

	// Rotulagem de blobs
	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			pos = y * bytesperline + x;
			if (datadst[pos] == 255) {
				int A = datadst[(y - 1) * bytesperline + (x - 1)];
				int B = datadst[(y - 1) * bytesperline + x];
				int C = datadst[(y - 1) * bytesperline + (x + 1)];
				int D = datadst[y * bytesperline + (x - 1)];
				int neighbors[4] = { A, B, C, D };
				int min = 0xFFFF;

				// Ordena os vizinhos
				for (i = 0; i < 4; i++) {
					if (neighbors[i] > 0 && neighbors[i] < min) min = neighbors[i];
				}

				// Se não houver vizinhos rotulados, atribui um novo rótulo
				if (min == 0xFFFF) {
					datadst[pos] = label;
					label++;
				}
				else {
					datadst[pos] = min;
					for (i = 0; i < 4; i++) {
						if (neighbors[i] > 0 && neighbors[i] != min) {
							int l1 = labeltable[neighbors[i]];
							int l2 = labeltable[min];
							while (labeltable[l1] != l1) l1 = labeltable[l1];
							while (labeltable[l2] != l2) l2 = labeltable[l2];
							if (l1 != l2) labeltable[l1] = l2;
						}
					}
				}
			}
		}
	}
	// Corrige os rótulos
	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			pos = y * bytesperline + x;
			if (datadst[pos] > 0) {
				int current = datadst[pos];
				while (labeltable[current] != labeltable[labeltable[current]]) {
					labeltable[current] = labeltable[labeltable[current]];
					current = labeltable[current];
				}
				datadst[pos] = labeltable[current];
			}
		}
	}
	// Reorganiza os rótulos
	*nlabels = 0;
	for (i = 1; i < label; i++) {
		if (labeltable[i] == i) {
			newlabels[i] = ++(*nlabels);
		}
	}

	// Atualiza os rótulos na imagem de saída
	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			pos = y * bytesperline + x;
			if (datadst[pos] > 0)
				datadst[pos] = newlabels[datadst[pos]];
		}
	}

	// Aloca memória para os blobs
	OVC* blobs = (OVC*)calloc(*nlabels, sizeof(OVC));
	if (!blobs) {
		free(labeltable);
		free(newlabels);
		return NULL;
	}
	int* sum_x = (int*)calloc(*nlabels, sizeof(int));
	int* sum_y = (int*)calloc(*nlabels, sizeof(int));

	// Inicializa os blobs
	for (i = 0; i < *nlabels; i++) {
		blobs[i].label = i + 1;
		blobs[i].area = 0;
		blobs[i].perimeter = 0;
		blobs[i].x = width;
		blobs[i].y = height;
		blobs[i].width = 0;
		blobs[i].height = 0;
	}

	// Calcula a área, perímetro e coordenadas dos blobs
	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			pos = y * bytesperline + x;
			int val = datadst[pos];
			if (val > 0) {
				int idx = val - 1;
				OVC* blob = &blobs[idx];
				blob->area++;
				sum_x[idx] += x;
				sum_y[idx] += y;

				// Atualiza as coordenadas do blob
				if (x < blob->x) blob->x = x;
				if (y < blob->y) blob->y = y;
				if (x > blob->x + blob->width) blob->width = x - blob->x;
				if (y > blob->y + blob->height) blob->height = y - blob->y;

				//Verificar perimetro
				if (datadst[(y - 1) * bytesperline + x] == 0 ||
					datadst[y * bytesperline + (x - 1)] == 0 ||
					datadst[y * bytesperline + (x + 1)] == 0 ||
					datadst[(y + 1) * bytesperline + x] == 0) {
					blob->perimeter++;
				}
			}
		}
	}

	// Corrige a largura e altura dos blobs
	for (i = 0; i < *nlabels; i++) {
		if (blobs[i].area > 0) {
			blobs[i].xc = sum_x[i] / blobs[i].area;
			blobs[i].yc = sum_y[i] / blobs[i].area;
			blobs[i].width++;
			blobs[i].height++;
		}
	}

	// Liberta memória
	free(labeltable);
	free(newlabels);
	free(sum_x);
	free(sum_y);
	return blobs;
}

/**
 * Desenha um retângulo em uma imagem, representando um blob rotulado.
 *
 * \param frame - ponteiro para a imagem onde o retângulo será desenhado
 * \param width - largura da imagem
 * \param height - altura da imagem
 * \param blobs - ponteiro para a lista de blobs rotulados
 * \param i - índice do blob a ser desenhado
 */
void draw_rectangle(unsigned char* frame, int width, int height, OVC* blobs, int i) {
	// Define a cor do retângulo (vermelho em BGR)
	unsigned char red[3] = { 0, 0, 255 };

	// Coordenadas e dimensões do retângulo
	int x = blobs[i].x;
	int y = blobs[i].y;
	int rect_width = blobs[i].width;
	int rect_height = blobs[i].height;

	// Desenha as linhas superior e inferior do retângulo (2-pixel thickness)
	for (int j = x - 1; j < x + rect_width + 1; j++) {
		if (j >= 0 && j < width) {
			if (y - 1 >= 0 && y - 1 < height) {
				frame[((y - 1) * width + j) * 3 + 0] = red[0];
				frame[((y - 1) * width + j) * 3 + 1] = red[1];
				frame[((y - 1) * width + j) * 3 + 2] = red[2];
			}
			if (y >= 0 && y < height) {
				frame[(y * width + j) * 3 + 0] = red[0];
				frame[(y * width + j) * 3 + 1] = red[1];
				frame[(y * width + j) * 3 + 2] = red[2];
			}
			if (y + rect_height >= 0 && y + rect_height < height) {
				frame[((y + rect_height) * width + j) * 3 + 0] = red[0];
				frame[((y + rect_height) * width + j) * 3 + 1] = red[1];
				frame[((y + rect_height) * width + j) * 3 + 2] = red[2];
			}
			if (y + rect_height + 1 >= 0 && y + rect_height + 1 < height) {
				frame[((y + rect_height + 1) * width + j) * 3 + 0] = red[0];
				frame[((y + rect_height + 1) * width + j) * 3 + 1] = red[1];
				frame[((y + rect_height + 1) * width + j) * 3 + 2] = red[2];
			}
		}
	}

	// Desenha as linhas laterais do retângulo (2-pixel thickness)
	for (int j = y - 1; j < y + rect_height + 1; j++) {
		if (j >= 0 && j < height) {
			if (x - 1 >= 0 && x - 1 < width) {
				frame[(j * width + x - 1) * 3 + 0] = red[0];
				frame[(j * width + x - 1) * 3 + 1] = red[1];
				frame[(j * width + x - 1) * 3 + 2] = red[2];
			}
			if (x >= 0 && x < width) {
				frame[(j * width + x) * 3 + 0] = red[0];
				frame[(j * width + x) * 3 + 1] = red[1];
				frame[(j * width + x) * 3 + 2] = red[2];
			}
			if (x + rect_width >= 0 && x + rect_width < width) {
				frame[(j * width + (x + rect_width)) * 3 + 0] = red[0];
				frame[(j * width + (x + rect_width)) * 3 + 1] = red[1];
				frame[(j * width + (x + rect_width)) * 3 + 2] = red[2];
			}
			if (x + rect_width + 1 >= 0 && x + rect_width + 1 < width) {
				frame[(j * width + (x + rect_width + 1)) * 3 + 0] = red[0];
				frame[(j * width + (x + rect_width + 1)) * 3 + 1] = red[1];
				frame[(j * width + (x + rect_width + 1)) * 3 + 2] = red[2];
			}
		}
	}

	// Desenha uma cruz no centro do retângulo, 6 pixels de extenção para cada direção
	int cx = x + rect_width / 2;
	int cy = y + rect_height / 2;
	int cross_len = 6;

	// Desenha a cruz
	for (int dx = -cross_len; dx <= cross_len; dx++) {
		int px = cx + dx;
		if (px >= 0 && px < width && cy >= 0 && cy < height) {
			frame[(cy * width + px) * 3 + 0] = red[0];
			frame[(cy * width + px) * 3 + 1] = red[1];
			frame[(cy * width + px) * 3 + 2] = red[2];
		}
	}

	// Desenha a cruz
	for (int dy = -cross_len; dy <= cross_len; dy++) {
		int py = cy + dy;
		if (py >= 0 && py < height && cx >= 0 && cx < width) {
			frame[(py * width + cx) * 3 + 0] = red[0];
			frame[(py * width + cx) * 3 + 1] = red[1];
			frame[(py * width + cx) * 3 + 2] = red[2];
		}
	}
}

/**
 * Calcula a distância entre dois pontos (x1, y1) e (x2, y2).
 *
 * \param x1 - coordenada x do primeiro ponto
 * \param y1 - coordenada y do primeiro ponto
 * \param x2 - coordenada x do segundo ponto
 * \param y2 - coordenada y do segundo ponto
 * \return - distância entre os dois pontos
 */
float distance(int x1, int y1, int x2, int y2) {
	return sqrtf((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}

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
bool isValidCircularBlob(OVC blob, float minCircularity, float minAspectRatio, float MaxAspectRacio, int minArea, int maxArea) {
	if (blob.perimeter <= 0) return false;

	// Calcular a circularidade
	float circularity = (4.0f * 3.14159265f * blob.area) / (blob.perimeter * blob.perimeter);
	if (circularity < minCircularity) return false;

	// Calcular a razão de aspecto
	float aspect_ratio = (float)blob.width / blob.height;
	if (aspect_ratio > MaxAspectRacio || aspect_ratio < minAspectRatio) return false;

	// Verificar a área
	if (blob.area < minArea || blob.area > maxArea) return false;

	return true;
}

/**
 * Identifica o metal de uma moeda com base em sua cor HSV.
 *
 * \param hsv_image - imagem HSV da moeda
 * \param blob - estrutura OVC representando o blob da moeda
 * \param gold - array com os limites de HSV para o ouro
 * \param silver - array com os limites de HSV para a prata
 * \param copper - array com os limites de HSV para o cobre
 * \param id - ID do blob
 * \return - valor representando o tipo de metal (0.05 = cobre, 0.20 = ouro, 1.0 = prata)
 */
float IdentifyCoinMetal(IVC* hsv_image, OVC blob, int* gold, int* silver, int* copper, int id) {
	// Se a imagem HSV e o blob são inválidos ou vazios, retorna 0.0f
	if (blob.width <= 0 || blob.height <= 0 || blob.area <= 0) return 0.0f;

	// Amostra de uma região central do blob
	int sample_width = blob.width / 3;
	int sample_height = blob.height / 3;
	int start_x = blob.x + (blob.width - sample_width) / 2;
	int start_y = blob.y + (blob.height - sample_height) / 2;

	// Valores de cor HSV acumulados
	int h_total = 0, s_total = 0, v_total = 0;
	int gold_pixels = 0, silver_pixels = 0, copper_pixels = 0;
	int total_pixels = 0;

	// Verificar pixeis da amostra
	for (int y = start_y; y < start_y + sample_height; y++) {
		for (int x = start_x; x < start_x + sample_width; x++) {
			
			// Verifica se o pixel está dentro dos limites da imagem
			if (x < 0 || x >= hsv_image->width || y < 0 || y >= hsv_image->height) continue;

			//Calcular posição do pixel na imagem HSV
			int offset = (y * hsv_image->width + x) * 3;

			// Obter os valores HSV do pixel
			int h = hsv_image->data[offset];
			int s = hsv_image->data[offset + 1];
			int v = hsv_image->data[offset + 2];

			// Normalizar os valores HSV
			float h_norm = (float)h * (360.0f / 255.0f);
			float s_norm = (float)s * (100.0f / 255.0f);
			float v_norm = (float)v * (100.0f / 255.0f);

			// Acumular os valores HSV
			h_total += h;
			s_total += s;
			v_total += v;

			// Identificação mais precisa com lógica de prioridade
			// Primeiro verificamos se é prata, pois é mais distintiva
			if (h_norm >= silver[0] && h_norm <= silver[1] &&
				s_norm >= silver[2] && s_norm <= silver[3] &&
				v_norm >= silver[4] && v_norm <= silver[5]) {
				silver_pixels++;
			}
			// Depois verificamos cobre, dando-lhe prioridade sobre ouro em caso de região de sobreposição
			else if (h_norm >= copper[0] && h_norm <= copper[1] &&
				s_norm >= copper[2] && s_norm <= copper[3] &&
				v_norm >= copper[4] && v_norm <= copper[5]) {
				// Verificação extra para diferenciação entre cobre e ouro nas regiões de sobreposição
				if (h_norm <= 35) {  // Se o matiz está mais próximo do vermelho, é provavelmente cobre
					copper_pixels++;
				}
				else {
					gold_pixels++;  // Senão, consideramos ouro
				}
			}
			// Se não for nenhum dos casos acima, mas estiver na faixa de ouro, consideramos ouro
			else if (h_norm >= gold[0] && h_norm <= gold[1] &&
				s_norm >= gold[2] && s_norm <= gold[3] &&
				v_norm >= gold[4] && v_norm <= gold[5]) {
				gold_pixels++;
			}

			total_pixels++;
		}
	}

	// Se não houver pixels válidos, retorna 0.0f
	if (total_pixels == 0) return 0.0f;

	// Calcular porcentagens de cada metal
	float gold_percent = (float)gold_pixels / total_pixels;
	float silver_percent = (float)silver_pixels / total_pixels;
	float copper_percent = (float)copper_pixels / total_pixels;

	// Definir um limiar para determinar a presença de cada metal
	const float THRESHOLD = 0.1f;

	// Lógica adicional para resolver sobreposições
	// Se estiver na região de sobreposição entre cobre e ouro, usamos um critério adicional
	if (copper_percent > 0.2f && gold_percent > 0.2f) {
		// Calcular a média do matiz (hue) dos pixels
		float h_mean = ((float)h_total / total_pixels) * (360.0f / 255.0f);

		// Se o matiz médio estiver mais próximo do vermelho, provavelmente é cobre
		if (h_mean < 35) {
			return 0.05f; // Copper
		}
		else {
			return 0.20f; // Gold
		}
	}
	// Se a porcentagem de um metal for maior que o limiar e maior que os outros, retorna o valor correspondente
	if (gold_percent > THRESHOLD && gold_percent > silver_percent && gold_percent > copper_percent) {
		return 0.20f; // Õuro
	}
	else if (silver_percent > THRESHOLD && silver_percent > gold_percent && silver_percent > copper_percent) {
		return 1.0f; // Prata
	}
	else if (copper_percent > THRESHOLD && copper_percent > gold_percent && copper_percent > silver_percent) {
		return 0.05f; // Cobre
	}

	// Se não houver uma classificação clara, use o valor HSV médio como fallback
	float h_mean = (float)h_total / total_pixels * (360.0f / 255.0f);
	float s_mean = (float)s_total / total_pixels * (100.0f / 255.0f);
	float v_mean = (float)v_total / total_pixels * (100.0f / 255.0f);

	// Mais um critério para resolver sobreposições: a relação entre saturação e valor
	if (h_mean >= 20 && h_mean <= 40) {  // Região de sobreposição no matiz
		if (s_mean > 50) {  // Cobre tende a ter saturação mais alta
			return 0.05f; // Copper
		}
		else {
			return 0.20f; // Gold
		}
	}

	// Verificação padrão de fallback
	if (h_mean >= gold[0] && h_mean <= gold[1] &&
		s_mean >= gold[2] && s_mean <= gold[3] &&
		v_mean >= gold[4] && v_mean <= gold[5]) {
		return 0.20f; // Ouro
	}
	else if (h_mean >= silver[0] && h_mean <= silver[1] &&
		s_mean >= silver[2] && s_mean <= silver[3] &&
		v_mean >= silver[4] && v_mean <= silver[5]) {
		return 1.0f; // Prata
	}
	else if (h_mean >= copper[0] && h_mean <= copper[1] &&
		s_mean >= copper[2] && s_mean <= copper[3] &&
		v_mean >= copper[4] && v_mean <= copper[5]) {
		return 0.05f; // Cobre
	}

	return 0.0f; // Se o metal for desconhecido retorna 0.0f
}

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
float GetCoinValue(OVC blob, IVC* hsv_image, int* gold, int* silver, int* copper, int* areas, int id) {
	// Verifica área de cada blob
	if (blob.area > areas[0] && blob.area < areas[1]) return 2.00f;
	if (blob.area > areas[2] && blob.area < areas[3]) return 0.50f;
	if (blob.area > areas[4] && blob.area < areas[5]) return IdentifyCoinMetal(hsv_image, blob, gold, silver, copper, id);
	if (blob.area > areas[6] && blob.area < areas[7]) return 0.10f;
	if (blob.area > areas[8] && blob.area < areas[9]) return 0.02f;
	if (blob.area > areas[10] && blob.area < areas[11]) return 0.01f;
	//Se a moeda não for reconhecida, retorna 0.0f
	return 0.0f;
}

/**
 * Exibe informações sobre uma moeda rastreada.
 *
 * \param coin - estrutura TrackedCoin representando a moeda
 */
void DisplayCoinInfo(TrackedCoin coin) {
	printf("Moeda nº%02d tem uma área de %d píxeis, perímetro de %d píxeis.\nTem o valor de %.2f€\n\n",
		coin.id, coin.area, coin.perimeter, coin.value);	
}

/**
 * Atualiza as informações de uma moeda rastreada.
 *
 * \param coin - ponteiro para a estrutura TrackedCoin representando a moeda
 * \param cx - coordenada x do centro da moeda
 * \param cy - coordenada y do centro da moeda
 */
void UpdateCoinFlags(TrackedCoin* coin, int cx, int cy) {
	coin->cx = cx;
	coin->cy = cy;
	coin->frames_seen++;
	coin->frames_missing = 0;
}

/**
 * Tenta adicionar uma nova moeda à lista de moedas rastreadas.
 *
 * \param trackedCoins - array de moedas rastreadas
 * \param coin_id_counter - contador de IDs de moedas
 * \param cx - coordenada x do centro da moeda
 * \param cy - coordenada y do centro da moeda
 * \param blob - estrutura OVC representando a moeda
 * \param unique_ids_seen - array de IDs únicos vistos
 * \param total_unique_coins - ponteiro para o número total de moedas únicas
 * \param total_value - ponteiro para o valor total das moedas
 * \param image_hsv - imagem HSV da moeda
 * \param hsvGold - limites HSV para o ouro
 * \param hsvSilver - limites HSV para a prata
 * \param hsvCopper - limites HSV para o cobre
 * \param coinAreas - array com os limites de área para diferentes moedas
 */
void TryAddNewCoin(TrackedCoin* trackedCoins, int* coin_id_counter, int cx, int cy, OVC blob, int* unique_ids_seen, 
int* total_unique_coins, float* total_value, IVC image_hsv, int* hsvGold, int* hsvSilver, int* hsvCopper, int* coinAreas) {
	// Verifica se a moeda já está ativa, se não, tenta adicionar uma nova moeda
	for (int j = 0; j < MAX_TRACKED_COINS; j++) {
		if (!trackedCoins[j].active) {
			trackedCoins[j].id = (*coin_id_counter)++;
			trackedCoins[j].cx = cx;
			trackedCoins[j].cy = cy;
			trackedCoins[j].perimeter = blob.perimeter;
			trackedCoins[j].area = blob.area;
			trackedCoins[j].frames_seen = 1;
			trackedCoins[j].frames_missing = 0;
			trackedCoins[j].active = 1;
			trackedCoins[j].value = GetCoinValue(blob, &image_hsv, hsvGold, hsvSilver, hsvCopper, coinAreas, trackedCoins[j].id);

			// Verifica se a moeda já foi contada
			int id = trackedCoins[j].id;
			int already_counted = 0;
			for (int k = 0; k < *total_unique_coins; k++) {
				if (unique_ids_seen[k] == id) {
					already_counted = 1;
					break;
				}
			}

			// Se a moeda não foi contada, adiciona ao array de IDs únicos
			if (!already_counted) {
				unique_ids_seen[(*total_unique_coins)++] = id;
				*total_value += trackedCoins[j].value;
				DisplayCoinInfo(trackedCoins[j]);
			}
			break;
		}
	}
}

/**
 * Atualiza o status das moedas rastreadas, marcando-as como inativas se não forem vistas por um certo número de quadros.
 *
 * \param trackedCoins - array de moedas rastreadas
 * \param max_tracked_coins - número máximo de moedas rastreadas
 * \param max_missing_frames - número máximo de frames sem verificação
 */
void UpdateTrackedCoinStatus(TrackedCoin* trackedCoins, int max_tracked_coins, int max_missing_frames) {
	for (int j = 0; j < max_tracked_coins; j++) {
		if (trackedCoins[j].active) {
			trackedCoins[j].frames_missing++;
			if (trackedCoins[j].frames_missing > max_missing_frames) {
				trackedCoins[j].active = 0;
			}
		}
	}
}

void SaveUniqueTrackedCoins(TrackedCoin* source, TrackedCoin* destination, int maxCoins) {
	for (int i = 0; i < maxCoins; i++) {
		if (source[i].active) {
			bool alreadySaved = false;

			// Verifica se a moeda já está guardada
			for (int j = 0; j < maxCoins; j++) {
				if (destination[j].active && destination[j].id == source[i].id) {
					alreadySaved = true;
					break;
				}
			}

			// Se a moeda não estiver guardada, adiciona-a à lista de moedas guardadas
			if (!alreadySaved) {
				for (int j = 0; j < maxCoins; j++) {
					if (!destination[j].active) {
						destination[j] = source[i];
						break;
					}
				}
			}
		}
	}
}

#pragma endregion
