//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//           INSTITUTO POLITÉCNICO DO CÁVADO E DO AVE
//                          2022/2023
//             ENGENHARIA DE SISTEMAS INFORMÁTICOS
//                    VISÃO POR COMPUTADOR
//
//             [  DUARTE DUQUE - dduque@ipca.pt  ]
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#ifndef VC_H   // Include guard start
#define VC_H

#include <stdio.h>  // Standard libraries
#include <stdlib.h>
#define VC_DEBUG


//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//                   ESTRUTURA DE UMA IMAGEM
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


typedef struct {
	unsigned char *data;
	int width, height;
	int channels;			// Binário/Cinzentos=1; RGB=3
	int levels;				// Binário=1; Cinzentos [1,255]; RGB [1,255]
	int bytesperline;		// width * channels
} IVC;

typedef struct {
	int x, y, width, height; // Caixa Delimitadora
	int area; // área
	int xc, yc; // Centro-de-massa
	int perimeter; // Perimetro
	int label; // Etiqueta
} OVC;

//typedef struct {
//	int id;
//	int cx, cy;
//	int frames_seen;
//	int frames_missing;
//	int active;
//} TrackedCoin;

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//                    PROTÓTIPOS DE FUNÇÕES
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#pragma region Standard_Functions
IVC *vc_image_new(int width, int height, int channels, int levels);
IVC *vc_image_free(IVC *image);

IVC *vc_read_image(char *filename);
int vc_write_image(char *filename, IVC *image);

int vc_gray_negative(IVC* srcdst);

int vc_rgb_get_red_gray(IVC* srcdst);
int vc_rgb_get_green_gray(IVC* srcdst);
int vc_rgb_get_blue_gray(IVC* srcdst);

int vc_rgb_to_gray(IVC* src, IVC* dst);
int vc_scale_gray_to_rgb(IVC* src, IVC* dst);

int vc_gray_to_binary(IVC* src, IVC* dst, int threshold);
int vc_gray_to_binary_global_mean(IVC* src, IVC* dst);

int vc_binary_midpoint(IVC* src, IVC* dst, int kernelSize);
int vc_bernsen_threshold(IVC* src, IVC* dst, int kernelSize, int contrastThreshold);
int vc_niblack_threshold(IVC* src, IVC* dst, int kernelSize, float k);

int vc_binary_dilation(IVC* src, IVC* dst, int kernelSize);
int vc_binary_erosion(IVC* src, IVC* dst, int kernelSize);

int vc_binary_open(IVC* src, IVC* dst, int kernelSize);
int vc_binary_close(IVC* src, IVC* dst, int kernelSize);

int vc_greyscale_dilation(IVC* src, IVC* dst, int kernelSize);
int vc_greyscale_erosion(IVC* src, IVC* dst, int kernelSize);
int vc_greyscale_open(IVC* src, IVC* dst, int kernelSize);
int vc_greyscale_close(IVC* src, IVC* dst, int kernelSize);
int vc_gray_edge_prewitt(IVC* src, IVC* dst, float th);
IVC* vc_gray_histogram_show(IVC* src);
int vc_gray_lowpass_median_filter(IVC* src, IVC* dst, int kernelSize);
int vc_gray_lowpass_mean_filter(IVC* src, IVC* dst, int kernelSize);
int vc_gray_lowpass_gaussian_filter(IVC* src, IVC* dst);
#pragma endregion

#pragma region Tp_Functions

int vc_hsv_to_segmentation_GoldSilverCopper(IVC* src, IVC* dst, int* hsvGold, int* silver, int* copper);
int vc_filter_circular_blobs(IVC* bin, OVC* blobs, int nblobs, float min_circularity, float max_circularity, int min_area, OVC* circular_blobs);
void vc_draw_blob_boundingbox(IVC* image, OVC blob, unsigned char r, unsigned char g, unsigned char b);
int vc_bgr_to_hsv(IVC* src, IVC* dst);
int vc_hsv_segmentation_to_binary(IVC* src, IVC* dst, int hmin, int hmax, int smin, int smax, int vmin, int vmax);
OVC* vc_binary_blob_labelling(IVC* src, IVC* dst, int* nlabels);
int vc_bgr_to_binary_with_threshold(IVC* src, IVC* dst, int threshold);
void draw_rectangle(IVC* image, int x, int y, int width, int height, int r, int g, int b);
int vc_binary_blob_info(IVC* src, OVC* blobs, int nblobs);
//float distance(int x1, int y1, int x2, int y2);
#pragma endregion
#endif // VC_H   // Include guard end