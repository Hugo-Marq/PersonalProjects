//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//           INSTITUTO POLITÉCNICO DO CÁVADO E DO AVE
//                          2024/2025
//             ENGENHARIA DE SISTEMAS INFORMÁTICOS
//                    VISÃO POR COMPUTADOR
//
//             [  DUARTE DUQUE - dduque@ipca.pt  ]
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

#pragma region standard_functions
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

int vc_gray_negative(IVC* srcdst)
{
	if (srcdst == NULL) return 0;

	for (int i = 0; i < srcdst->bytesperline * srcdst->height; i += srcdst->channels) {
		srcdst->data[i] = 255 - srcdst->data[i];
	}
	return 1;
}

int vc_rgb_get_red_gray(IVC* srcdst) {
	int width = srcdst->width;
	int height = srcdst->height;
	int channels = srcdst->channels;
	int bytesperline = srcdst->bytesperline;
	int x, y;
	long int pos;
	unsigned char* data = (unsigned char*)srcdst->data;

	if ((srcdst->width <= 0) || (srcdst->height <= 0) || srcdst->data == NULL) return 0;
	if (channels != 3) return 0;
	if (srcdst->channels == 3) {
		for (y = 0; y < height; y++)
			for (x = 0; x < width; x++) {
				pos = y * srcdst->bytesperline + x * srcdst->channels;
				data[pos] = data[pos];
				data[pos + 1] = data[pos];
				data[pos + 2] = data[pos];
			}
	}
	return 1;
}

int vc_rgb_get_green_gray(IVC* srcdst) {
	int width = srcdst->width;
	int height = srcdst->height;
	int channels = srcdst->channels;
	int bytesperline = srcdst->bytesperline;
	int x, y;
	long int pos;
	unsigned char* data = (unsigned char*)srcdst->data;

	if ((srcdst->width <= 0) || (srcdst->height <= 0) || srcdst->data == NULL) return 0;
	if (channels != 3) return 0;
	if (srcdst->channels == 3) {
		for (y = 0; y < height; y++)
			for (x = 0; x < width; x++) {
				pos = y * srcdst->bytesperline + x * srcdst->channels;
				data[pos] = data[pos + 1];
				data[pos + 1] = data[pos + 1];
				data[pos + 2] = data[pos + 1];
			}
	}
	return 1;
}

int vc_rgb_get_blue_gray(IVC* srcdst) {
	int width = srcdst->width;
	int height = srcdst->height;
	int channels = srcdst->channels;
	int bytesperline = srcdst->bytesperline;
	int x, y;
	long int pos;
	unsigned char* data = (unsigned char*)srcdst->data;

	if ((srcdst->width <= 0) || (srcdst->height <= 0) || srcdst->data == NULL) return 0;
	if (channels != 3) return 0;
	if (srcdst->channels == 3) {
		for (y = 0; y < height; y++)
			for (x = 0; x < width; x++) {
				pos = y * srcdst->bytesperline + x * srcdst->channels;
				data[pos] = data[pos + 2];
				data[pos + 1] = data[pos + 2];
				data[pos + 2] = data[pos + 2];
			}
	}
	return 1;
}

int vc_rgb_to_gray(IVC* src, IVC* dst)
{
	unsigned char* datasrc = (unsigned char*)src->data;
	int bytesperline_src = src->width * src->channels;
	int channels_src = src->channels;
	unsigned char* datadst = (unsigned*)dst->data;
	int bytesperline_dst = dst->width * dst->channels;
	int channels_dst = dst->channels;

	int width = src->width;
	int height = src->height;
	int x, y;
	long int pos_src, pos_dst;
	float rf, gf, bf;

	if ((src->width <= 0) || (src->height <= 0) || (src->data == NULL)) return 0;
	if ((src->width != dst->width) || (src->height != dst->height)) return 0;
	if ((src->channels != 3) || (dst->channels != 1)) return 0;

	for (y = 0; y < height; y++)
	{
		for (x = 0; x < width; x++)
		{
			pos_src = y * bytesperline_src + x * channels_src;
			pos_dst = y * bytesperline_dst + x * channels_dst;

			rf = (float)datasrc[pos_src];
			gf = (float)datasrc[pos_src + 1];
			bf = (float)datasrc[pos_src + 2];

			datadst[pos_dst] = (unsigned char)((rf * 0.2999) + (gf * 0.587) + (bf * 0.114));
		}
	}
	return 1;
}

int vc_scale_gray_to_rgb(IVC* src, IVC* dst) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int width = src->width;
	int height = src->height;
	int x, y;
	long int gray_pos, rgb_pos;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 3) return 0;

	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			gray_pos = y * width + x;        // Position in grayscale image
			rgb_pos = (y * width + x) * 3;   // Position in RGB image
			unsigned char gray = datasrc[gray_pos];  // Store grayscale value

			// Mapping grayscale values to RGB
			if (gray < 64) {
				datadst[rgb_pos] = 0;
				datadst[rgb_pos + 1] = gray * 4;  // Scale to keep a smooth gradient
				datadst[rgb_pos + 2] = 255;
			}
			else if (gray < 128) {
				datadst[rgb_pos] = 0;
				datadst[rgb_pos + 1] = 255;
				datadst[rgb_pos + 2] = (128 - gray) * 4; // Inverted smooth gradient
			}
			else if (gray < 192) {
				datadst[rgb_pos] = (gray - 128) * 4;  // Smooth transition from green to red
				datadst[rgb_pos + 1] = 255;
				datadst[rgb_pos + 2] = 0;
			}
			else {
				datadst[rgb_pos] = 255;
				datadst[rgb_pos + 1] = (255 - gray) * 4;  // Gradual fade of green
				datadst[rgb_pos + 2] = 0;
			}
		}
	}
	return 1;
}

int vc_gray_to_binary_global_mean(IVC* src, IVC* dst) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int bytesperline = src->width * src->channels;
	int channels = src->channels;
	int width = src->width;
	int height = src->height;
	int x, y;
	long int pos;
	long sum = 0;
	float mean;
	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			pos = y * bytesperline + x * channels;
			sum += datasrc[pos];
		}
	}
	mean = (float)sum / (width * height);
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			pos = y * width + x;  // Position in grayscale image
			unsigned char gray = datasrc[pos];  // Store grayscale value
			// Thresholding
			if (gray < mean) {
				datadst[y * width + x] = 0;  // Black
			}
			else {
				datadst[y * width + x] = 255;  // White
			}
		}
	}
	return 1;
}

int vc_binary_midpoint(IVC* src, IVC* dst, int kernelSize) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky, k, l;
	long int pos;
	int offset = (kernelSize - 1) / 2;
	int value, min, max;
	unsigned char threshold;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			min = src->levels - 1;
			max = 0;
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] < min) min = datasrc[pos];
						if (datasrc[pos] > max) max = datasrc[pos];
					}
				}
			}
			threshold = (unsigned char)((min / 2) + (max / 2));

			pos = y * bytesperline + x * channels;
			if (datasrc[pos] < threshold)
			{
				datadst[pos] = 0;
			}
			else
			{
				datadst[pos] = 255;
			}
		}
	}
	return 1;
}

int vc_bernsen_threshold(IVC* src, IVC* dst, int kernelSize, int contrastThreshold) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;
	int min, max, contrast;
	unsigned char threshold;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			min = src->levels - 1;
			max = 0;
			// Loop through the kernel
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] < min) min = datasrc[pos];
						if (datasrc[pos] > max) max = datasrc[pos];
					}
				}
			}
			contrast = max - min;

			// Apply the Bernsen threshold based on local contrast
			if (contrast < contrastThreshold) {
				threshold = (unsigned char)((min + max) / 2);
			}
			else {
				threshold = datasrc[y * bytesperline + x * channels];
			}

			pos = y * bytesperline + x * channels;
			if (datasrc[pos] < threshold) {
				datadst[pos] = 0;
			}
			else {
				datadst[pos] = 255;
			}
		}
	}
	return 1;
}

int vc_niblack_threshold(IVC* src, IVC* dst, int kernelSize, float k) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;
	int sum;
	float mean, variance, stddev, threshold;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			sum = 0;
			float sumsq = 0.0f;
			// Loop through the kernel
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						sum += datasrc[pos];
						sumsq += datasrc[pos] * datasrc[pos];
					}
				}
			}

			// Calculate the mean and standard deviation of the neighborhood
			int totalPixels = kernelSize * kernelSize;
			mean = (float)sum / totalPixels;
			variance = (float)(sumsq / totalPixels - mean * mean);
			stddev = sqrt(variance);

			// Apply the Niblack threshold
			threshold = mean + k * stddev;

			// Assign the thresholded pixel
			pos = y * bytesperline + x * channels;
			if (datasrc[pos] < threshold) {
				datadst[pos] = 0;
			}
			else {
				datadst[pos] = 255;
			}
		}
	}
	return 1;
}

int vc_binary_open(IVC* src, IVC* dst, int kernelSize) {
	IVC* temp = vc_image_new(src->width, src->height, src->channels, src->levels);
	if (!temp) return 0;
	if (!vc_binary_erosion(src, temp, kernelSize)) return 0;
	if (!vc_binary_dilation(temp, dst, kernelSize)) return 0;
	vc_image_free(temp);
	return 1;
}

int vc_binary_close(IVC* src, IVC* dst, int kernelSize) {
	IVC* temp = vc_image_new(src->width, src->height, src->channels, src->levels);
	if (!temp) return 0;
	if (!vc_binary_dilation(src, temp, kernelSize)) return 0;
	if (!vc_binary_erosion(temp, dst, kernelSize)) return 0;
	vc_image_free(temp);
	return 1;
}

int vc_greyscale_dilation(IVC* src, IVC* dst, int kernelSize) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Perform binary dilation
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			unsigned char dilated = 0;
			// Loop through the kernel
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] > dilated) { // If any pixel in the kernel is greater than dilated, set dilated to that pixel value
							dilated = datasrc[pos];
						}
					}
				}
			}
			pos = y * bytesperline + x * channels;
			datadst[pos] = dilated;
		}
	}
	return 1;
}

int vc_greyscale_erosion(IVC* src, IVC* dst, int kernelSize) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Perform binary dilation
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			unsigned char eroded = src->levels - 1;
			// Loop through the kernel
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] < eroded) { // If any pixel in the kernel is lesser than eroded, set eroded to that pixel value
							eroded = datasrc[pos];
						}
					}
				}
			}
			pos = y * bytesperline + x * channels;
			datadst[pos] = eroded;
		}
	}
	return 1;
}

int vc_greyscale_open(IVC* src, IVC* dst, int kernelSize) {
	IVC* temp = vc_image_new(src->width, src->height, src->channels, src->levels);
	if (!temp) return 0;
	if (!vc_greyscale_erosion(src, temp, kernelSize)) return 0;
	if (!vc_greyscale_dilation(temp, dst, kernelSize)) return 0;
	vc_image_free(temp);
	return 1;
}

int vc_greyscale_close(IVC* src, IVC* dst, int kernelSize) {
	IVC* temp = vc_image_new(src->width, src->height, src->channels, src->levels);
	if (!temp) return 0;
	if (!vc_greyscale_dilation(src, temp, kernelSize)) return 0;
	if (!vc_greyscale_erosion(temp, dst, kernelSize)) return 0;
	vc_image_free(temp);
	return 1;
}

IVC* vc_gray_histogram_show(IVC* src) {
	// Error checking
	if (src == NULL || src->data == NULL) return NULL;
	if (src->width <= 0 || src->height <= 0 || src->channels != 1) return NULL;

	unsigned char* datasrc = (unsigned char*)src->data;
	int width = src->width;
	int height = src->height;
	int channels = src->channels;
	int bytesperline = width * channels;
	long int pos;
	int x, y, i;

	// Create a new image for histogram display (256x256 grayscale)
	IVC* dst = vc_image_new(256, 256, 1, 255);
	if (dst == NULL) return NULL;

	unsigned char* datadst = (unsigned char*)dst->data;
	int hist[256] = { 0 };
	float histmax = 0.0f;
	long int nPixels = width * height;

	// Count the frequency of each grayscale level
	for (pos = 0; pos < nPixels; pos++) {
		hist[datasrc[pos]]++;
	}

	// Find max histogram value for normalization
	for (i = 0; i < 256; i++) {
		if (hist[i] > histmax) histmax = (float)hist[i];
	}

	// Clear destination image (set all pixels to 255 = white)
	for (pos = 0; pos < 256 * 256; pos++) {
		datadst[pos] = 255;
	}

	// Draw histogram as vertical lines
	for (x = 0; x < 256; x++) {
		int barHeight = (int)((hist[x] / histmax) * 255.0f);  // Scale to image height
		for (y = 255; y >= 255 - barHeight; y--) {
			datadst[y * 256 + x] = 0;  // Draw in black
		}
	}

	return dst;
}

int vc_gray_edge_prewitt(IVC* src, IVC* dst, float th) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int width = src->width;
	int height = src->height;
	int channels = src->channels;
	int bytesperline = width * channels;
	long int pos;
	int x, y;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (width <= 0 || height <= 0) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Initialize destination image
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			pos = y * bytesperline + x * channels;
			datadst[pos] = 0; // Set all pixels to black initially
		}
	}

	// Prewitt operator kernels
	int Gx[3][3] = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
	int Gy[3][3] = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

	// Apply Prewitt operator
	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			float sumX = 0.0f;
			float sumY = 0.0f;

			// Convolution with Prewitt kernels
			for (int ky = -1; ky <= 1; ky++) {
				for (int kx = -1; kx <= 1; kx++) {
					pos = (y + ky) * bytesperline + (x + kx) * channels;
					sumX += datasrc[pos] * Gx[ky + 1][kx + 1];
					sumY += datasrc[pos] * Gy[ky + 1][kx + 1];
				}
			}

			float magnitude = sqrt(sumX * sumX + sumY * sumY);

			// Thresholding
			pos = y * bytesperline + x * channels;
			if (magnitude > th) {
				datadst[pos] = 255; // Edge detected
			}
			else {
				datadst[pos] = 0; // No edge
			}
		}
	}

	return 1;
}

int vc_gray_lowpass_mean_filter(IVC* src, IVC* dst, int kernel) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernel - 1) / 2;
	float mean = 0.0;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Perform binary dilation
	for (y = offset; y < height - offset; y++) {
		for (x = offset; x < width - offset; x++) {
			float mean = 0.0;
			// Loop through the kernel
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					pos = (y + ky) * bytesperline + (x + kx) * channels;
					mean += datasrc[pos];
				}
			}
			mean /= (kernel * kernel);
			if (mean < 0) mean = 0;
			if (mean > 255) mean = 255;
			pos = y * bytesperline + x * channels;
			datadst[pos] = (unsigned char)mean;
		}
	}
	return 1;
}

// Comparison function for qsort
int compare_uchar(const void* a, const void* b) {
	return (*(unsigned char*)a - *(unsigned char*)b);
}

int vc_gray_lowpass_median_filter(IVC* src, IVC* dst, int kernel) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernel - 1) / 2;
	int windowSize = kernel * kernel;
	int mid = windowSize / 2;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;
	if (kernel % 2 == 0 || kernel < 1) return 0; // kernel must be odd and >= 1

	// Temporary array to hold kernel values
	unsigned char* window = (unsigned char*)malloc(sizeof(unsigned char) * windowSize);
	if (window == NULL) return 0;

	// Apply median filter
	for (y = offset; y < height - offset; y++) {
		for (x = offset; x < width - offset; x++) {
			int count = 0;

			// Collect neighborhood values
			for (ky = -offset; ky <= offset; ky++) {
				for (kx = -offset; kx <= offset; kx++) {
					pos = (y + ky) * bytesperline + (x + kx) * channels;
					window[count++] = datasrc[pos];
				}
			}

			// Sort the window
			qsort(window, windowSize, sizeof(unsigned char), compare_uchar);

			// Set the median value to the destination
			pos = y * bytesperline + x * channels;
			datadst[pos] = window[mid];
		}
	}

	free(window);
	return 1;
}

int vc_gray_lowpass_gaussian_filter(IVC* src, IVC* dst) {
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int width = src->width;
	int height = src->height;
	int channels = src->channels;
	int bytesperline = width * channels;
	int offset = 2;

	const float gauss5[5] = { 0.0545f, 0.2442f, 0.4026f, 0.2442f, 0.0545f };

	for (int y = offset; y < height - offset; y++) {
		for (int x = offset; x < width - offset; x++) {
			float sum = 0.0f;

			for (int ky = -offset; ky <= offset; ky++) {
				for (int kx = -offset; kx <= offset; kx++) {
					float weight = gauss5[ky + offset] * gauss5[kx + offset];
					int pos = (y + ky) * bytesperline + (x + kx) * channels;
					sum += datasrc[pos] * weight;
				}
			}

			int pos = y * bytesperline + x * channels;
			if (sum < 0) sum = 0;
			if (sum > 255) sum = 255;
			datadst[pos] = (unsigned char)(sum + 0.5f);
		}
	}

	return 1;
}

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

int vc_hsv_segmentation_to_binary(IVC* src, IVC* dst, int hmin, int hmax, int smin, int smax, int vmin, int vmax) {
	// Verifica se as imagens de origem e destino são válidas e se os canais são apropriados
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 3 || dst->channels != 1) return 0;

	unsigned char* datasrc = (unsigned char*)src->data;  // Dados da imagem de origem
	unsigned char* datadst = (unsigned char*)dst->data;  // Dados da imagem de destino

	// Variáveis para iteração
	int x, y;
	long int pos, pos_dst;
	// Variáveis para armazenar valores de hue, saturation e value
	//hue [0, 360]º - saturation [0, 100]% - value [0, 100]%
	int hue, saturation, value;

	// Percorre cada pixel da imagem
	for (y = 0; y < src->height; y++) {
		for (x = 0; x < src->width; x++) {
			pos = y * src->bytesperline + x * src->channels; // Posição na imagem de origem
			pos_dst = y * dst->bytesperline + x * dst->channels; // Posição na imagem de destino

			// Extrai valores de hue, saturation e value do pixel e converte para faixas utilizáveis
			hue = (int)(((float)datasrc[pos]) * 360.0f / 255.0f);
			saturation = (int)(((float)datasrc[pos + 1]) * 100.0f / 255.0f);
			value = (int)(((float)datasrc[pos + 2]) * 100.0f / 255.0f);

			// Verifica se o pixel atual está dentro dos intervalos especificados para H, S e V
			if ((hue >= hmin && hue <= hmax) &&
				(saturation >= smin && saturation <= smax) &&
				(value >= vmin && value <= vmax)) {
				datadst[pos_dst] = 255;  // Pixel atende aos critérios, definido como branco
			}
			else {
				datadst[pos_dst] = 0;    // Pixel não atende aos critérios, definido como preto
			}
		}
	}
	return 1; // Retorna sucesso após a conversão
}

#pragma endregion

#pragma region tp_functions

int vc_hsv_to_segmentation_GoldSilverCopper(IVC* src, IVC* dst, int* gold, int* silver, int* copper) {

	unsigned char* datasrc = (unsigned char*)src->data;
	int bytesperline = src->width * src->channels;  // Source is 3-channel HSV
	int width = src->width;
	int height = src->height;
	unsigned char* datadst = (unsigned char*)dst->data; // Destination is 1-channel binary

	// Error checking
	if ((src->width <= 0) || (src->height <= 0) || (src->data == NULL)) return 0;
	if ((src->width != dst->width) || (src->height != dst->height)) return 0;
	if ((src->channels != 3) || (dst->channels != 1)) return 0;

	for (int y = 0; y < height; y++) {
		for (int x = 0; x < width; x++) {
			int pos = y * bytesperline + x * 3; // HSV has 3 channels

			int h = (int)((float)datasrc[pos] * (360.0f / 255.0f));  // Convert back to 0-360
			int s = (int)((float)datasrc[pos + 1] * (100.0f / 255.0f));  // Convert back to 0-100
			int v = (int)((float)datasrc[pos + 2] * (100.0f / 255.0f));  // Convert back to 0-100

			// Check if the pixel is within the HSV threshold ranges and manual correction

			if (h >= gold[0] && h <= gold[1] && s >= gold[2] && s <= gold[3] && v >= gold[4] && v <= gold[5]) {
				datadst[y * width + x] = 255; // Foreground (White)
			}
			else if (h >= silver[0] && h <= silver[1] && s >= silver[2] && s <= silver[3] && v >= silver[4] && v <= silver[5]) {
				datadst[y * width + x] = 255; // Foreground (White)
			}
			else if (h >= copper[0] && h <= copper[1] && s >= copper[2] && s <= copper[3] && v >= copper[4] && v <= copper[5]) {
				datadst[y * width + x] = 255; // Foreground (White)
			}
			else{
				datadst[y * width + x] = 0;   // Background (Black)
			}
		}
	}
	return 1;
}

void vc_draw_blob_boundingbox(IVC* image, OVC blob, unsigned char r, unsigned char g, unsigned char b) {
	int x1 = blob.x;
	int y1 = blob.y;
	int x2 = blob.x + blob.width - 1;
	int y2 = blob.y + blob.height - 1;

	for (int x = x1; x <= x2; x++) {
		image->data[(y1 * image->width + x) * 3 + 0] = r;
		image->data[(y1 * image->width + x) * 3 + 1] = g;
		image->data[(y1 * image->width + x) * 3 + 2] = b;

		image->data[(y2 * image->width + x) * 3 + 0] = r;
		image->data[(y2 * image->width + x) * 3 + 1] = g;
		image->data[(y2 * image->width + x) * 3 + 2] = b;
	}

	for (int y = y1; y <= y2; y++) {
		image->data[(y * image->width + x1) * 3 + 0] = r;
		image->data[(y * image->width + x1) * 3 + 1] = g;
		image->data[(y * image->width + x1) * 3 + 2] = b;

		image->data[(y * image->width + x2) * 3 + 0] = r;
		image->data[(y * image->width + x2) * 3 + 1] = g;
		image->data[(y * image->width + x2) * 3 + 2] = b;
	}
}

int vc_filter_circular_blobs(IVC* labeled, OVC* blobs, int nblobs, float min_circularity, float max_circularity, int min_area, OVC* circular_blobs) {
	int count = 0;

	for (int i = 0; i < nblobs; i++) {
		int label = blobs[i].label;
		int minx = labeled->width, maxx = 0, miny = labeled->height, maxy = 0, area = 0;
		int sumx = 0, sumy = 0;

		for (int y = 0; y < labeled->height; y++) {
			for (int x = 0; x < labeled->width; x++) {
				int pos = y * labeled->bytesperline + x;
				if (labeled->data[pos] == label) {
					area++;
					sumx += x;
					sumy += y;

					if (x < minx) minx = x;
					if (x > maxx) maxx = x;
					if (y < miny) miny = y;
					if (y > maxy) maxy = y;
				}
			}
		}

		int w = maxx - minx + 1;
		int h = maxy - miny + 1;
		float circularity = (float)area / (w * h);

		if (circularity >= min_circularity && circularity <= max_circularity && area >= min_area) {
			OVC blob = blobs[i];
			blob.area = area;
			blob.x = minx;
			blob.y = miny;
			blob.width = w;
			blob.height = h;
			blob.xc = sumx / area;
			blob.yc = sumy / area;
			circular_blobs[count++] = blob;
		}
	}

	return count;
}

int vc_binary_dilation(IVC* src, IVC* dst, int kernelSize) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Perform binary dilation
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			int dilated = 0;
			// Loop through the kernel
			for (ky = -offset; ky <= offset && !dilated; ky++) {
				for (kx = -offset; kx <= offset && !dilated; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] == 255) {
							dilated = 1;
						}
					}
				}
			}
			pos = y * bytesperline + x * channels;
			datadst[pos] = (dilated ? 255 : 0);
		}
	}
	return 1;
}

int vc_binary_erosion(IVC* src, IVC* dst, int kernelSize) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int channels = src->channels;
	int bytesperline = src->width * src->channels;
	int width = src->width;
	int height = src->height;
	int x, y, kx, ky;
	long int pos;
	int offset = (kernelSize - 1) / 2;

	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;

	// Perform binary erosion
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			int eroded = 1;
			// Loop through the kernel
			for (ky = -offset; ky <= offset && eroded; ky++) {
				for (kx = -offset; kx <= offset && eroded; kx++) {
					if (((x + kx) >= 0) && ((x + kx) < width) && ((y + ky) >= 0) && ((y + ky) < height)) {
						pos = (y + ky) * bytesperline + (x + kx) * channels;
						if (datasrc[pos] == 0) {
							eroded = 0;
						}
					}
					else {
						eroded = 0;
					}
				}
			}
			pos = y * bytesperline + x * channels;
			datadst[pos] = (eroded ? 255 : 0);
		}
	}
	return 1;
}

int vc_gray_to_binary(IVC* src, IVC* dst, int threshold) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int width = src->width;
	int height = src->height;
	int x, y;
	long int pos;
	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 1 || dst->channels != 1) return 0;
	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {
			pos = y * width + x;  // Position in grayscale image
			unsigned char gray = datasrc[pos];  // Store grayscale value
			// Thresholding
			if (gray < threshold) {
				datadst[y * width + x] = 255;  // White
			}
			else {
				datadst[y * width + x] = 0;  // Black
			}
		}
	}
	return 1;
}

int vc_bgr_to_binary_with_threshold(IVC* src, IVC* dst, int threshold) {
	unsigned char* datasrc = (unsigned char*)src->data;
	unsigned char* datadst = (unsigned char*)dst->data;
	int width = src->width;
	int height = src->height;
	int x, y;
	long int pos;
	int bytesperline_src = src->width * src->channels;
	int bytesperline_dst = dst->width * dst->channels;
	int channels_src = src->channels;
	int channels_dst = dst->channels;
	long int pos_src, pos_dst;
	float rf, gf, bf;
	// Error checking
	if (!src || !dst || !src->data || !dst->data) return 0;
	if (src->width <= 0 || src->height <= 0) return 0;
	if (src->width != dst->width || src->height != dst->height) return 0;
	if (src->channels != 3 || dst->channels != 1) return 0;

	if ((src->width <= 0) || (src->height <= 0) || (src->data == NULL)) return 0;
	if ((src->width != dst->width) || (src->height != dst->height)) return 0;
	if ((src->channels != 3) || (dst->channels != 1)) return 0;

	for (y = 0; y < height; y++) {
		for (x = 0; x < width; x++) {

			pos_src = y * bytesperline_src + x * channels_src;
			pos_dst = y * bytesperline_dst + x * channels_dst;

			rf = (float)datasrc[pos_src];
			gf = (float)datasrc[pos_src + 1];
			bf = (float)datasrc[pos_src + 2];

			datadst[pos_dst] = (unsigned char)((rf * 0.2999) + (gf * 0.587) + (bf * 0.114));
			if (datadst[pos_dst] < threshold) {
				datadst[pos_dst] = 255;  // White
			}
			else {
				datadst[pos_dst] = 0;  // Black
			}
		}
	}
	return 1;
}

OVC* vc_binary_blob_labelling(IVC* src, IVC* dst, int* nlabels) {
	unsigned char* datasrc = src->data;
	unsigned char* datadst = dst->data;
	int width = src->width, height = src->height, channels = src->channels;
	int bytesperline = width * channels;
	int size = bytesperline * height;
	int label = 1, i, x, y, pos;

	if (!src || !dst || !datasrc || !datadst || width <= 0 || height <= 0 || channels != 1 ||
		src->width != dst->width || src->height != dst->height || src->channels != dst->channels) return NULL;

	int maxlabels = width * height;
	int* labeltable = (int*)malloc(sizeof(int) * maxlabels);
	int* newlabels = (int*)calloc(maxlabels, sizeof(int));
	if (!labeltable || !newlabels) return NULL;

	for (i = 0; i < size; i++) datadst[i] = (datasrc[i] != 0) ? 255 : 0;

	for (x = 0; x < width; x++) {
		datadst[x] = 0;
		datadst[(height - 1) * bytesperline + x] = 0;
	}
	for (y = 0; y < height; y++) {
		datadst[y * bytesperline] = 0;
		datadst[y * bytesperline + (width - 1)] = 0;
	}

	for (i = 0; i < maxlabels; i++) labeltable[i] = i;

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

				for (i = 0; i < 4; i++) {
					if (neighbors[i] > 0 && neighbors[i] < min) min = neighbors[i];
				}

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

	*nlabels = 0;
	for (i = 1; i < label; i++) {
		if (labeltable[i] == i) {
			newlabels[i] = ++(*nlabels);
		}
	}

	for (y = 1; y < height - 1; y++) {
		for (x = 1; x < width - 1; x++) {
			pos = y * bytesperline + x;
			if (datadst[pos] > 0)
				datadst[pos] = newlabels[datadst[pos]];
		}
	}

	OVC* blobs = (OVC*)calloc(*nlabels, sizeof(OVC));
	if (!blobs) {
		free(labeltable);
		free(newlabels);
		return NULL;
	}

	// Initialize accumulators
	int* sum_x = (int*)calloc(*nlabels, sizeof(int));
	int* sum_y = (int*)calloc(*nlabels, sizeof(int));

	for (i = 0; i < *nlabels; i++) {
		blobs[i].label = i + 1;
		blobs[i].area = 0;
		blobs[i].perimeter = 0;
		blobs[i].x = width;
		blobs[i].y = height;
		blobs[i].width = 0;
		blobs[i].height = 0;
	}

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

				if (x < blob->x) blob->x = x;
				if (y < blob->y) blob->y = y;
				if (x > blob->x + blob->width) blob->width = x - blob->x;
				if (y > blob->y + blob->height) blob->height = y - blob->y;

				// Perimeter check (4-neighbor)
				if (datadst[(y - 1) * bytesperline + x] == 0 ||
					datadst[y * bytesperline + (x - 1)] == 0 ||
					datadst[y * bytesperline + (x + 1)] == 0 ||
					datadst[(y + 1) * bytesperline + x] == 0) {
					blob->perimeter++;
				}
			}
		}
	}

	for (i = 0; i < *nlabels; i++) {
		if (blobs[i].area > 0) {
			blobs[i].xc = sum_x[i] / blobs[i].area;
			blobs[i].yc = sum_y[i] / blobs[i].area;
			blobs[i].width++;
			blobs[i].height++;
		}
	}

	free(labeltable);
	free(newlabels);
	free(sum_x);
	free(sum_y);
	return blobs;
}

int vc_binary_blob_info(IVC* src, OVC* blobs, int nblobs) {
	unsigned char* data = (unsigned char*)src->data;
	int width = src->width;
	int height = src->height;
	int bytesperline = src->bytesperline;
	int channels = src->channels;
	int x, y, i;
	long int pos;
	int xmin, ymin, xmax, ymax;
	long int sumx, sumy;

	// Safety checks
	if ((src == NULL) || (src->data == NULL) || (blobs == NULL)) return 0;
	if ((width <= 0) || (height <= 0) || (channels != 1)) return 0;

	for (i = 0; i < nblobs; i++) {
		xmin = width - 1;
		ymin = height - 1;
		xmax = 0;
		ymax = 0;

		sumx = 0;
		sumy = 0;

		blobs[i].area = 0;
		blobs[i].perimeter = 0;  // ← Fix: initialize perimeter to 0

		for (y = 1; y < height - 1; y++) {
			for (x = 1; x < width - 1; x++) {
				pos = y * bytesperline + x * channels;

				if (data[pos] == blobs[i].label) {
					// Area
					blobs[i].area++;

					// Center of mass sums
					sumx += x;
					sumy += y;

					// Bounding box
					if (x < xmin) xmin = x;
					if (y < ymin) ymin = y;
					if (x > xmax) xmax = x;
					if (y > ymax) ymax = y;

					// Perimeter check (4-connectivity)
					if (data[pos - channels] != blobs[i].label ||
						data[pos + channels] != blobs[i].label ||
						data[pos - bytesperline] != blobs[i].label ||
						data[pos + bytesperline] != blobs[i].label ||
						data[pos - bytesperline - channels] != blobs[i].label ||
						data[pos - bytesperline + channels] != blobs[i].label ||
						data[pos + bytesperline - channels] != blobs[i].label ||
						data[pos + bytesperline + channels] != blobs[i].label)
					{
						blobs[i].perimeter++;
					}

				}
			}
		}

		// Final bounding box values
		blobs[i].x = xmin;
		blobs[i].y = ymin;
		blobs[i].width = (xmax - xmin) + 1;
		blobs[i].height = (ymax - ymin) + 1;

		// Center of mass (avoid division by zero)
		blobs[i].xc = (blobs[i].area > 0) ? sumx / blobs[i].area : 0;
		blobs[i].yc = (blobs[i].area > 0) ? sumy / blobs[i].area : 0;
	}

	return 1;
}

void draw_rectangle(IVC* image, int x, int y, int width, int height, int r, int g, int b) {
	int i;

	for (i = x; i < x + width; i++) {
		// Top line
		image->data[(y * image->width + i) * 3 + 0] = r;
		image->data[(y * image->width + i) * 3 + 1] = g;
		image->data[(y * image->width + i) * 3 + 2] = b;

		// Bottom line
		image->data[((y + height) * image->width + i) * 3 + 0] = r;
		image->data[((y + height) * image->width + i) * 3 + 1] = g;
		image->data[((y + height) * image->width + i) * 3 + 2] = b;
	}

	for (i = y; i < y + height; i++) {
		// Left line
		image->data[(i * image->width + x) * 3 + 0] = r;
		image->data[(i * image->width + x) * 3 + 1] = g;
		image->data[(i * image->width + x) * 3 + 2] = b;

		// Right line
		image->data[(i * image->width + (x + width)) * 3 + 0] = r;
		image->data[(i * image->width + (x + width)) * 3 + 1] = g;
		image->data[(i * image->width + (x + width)) * 3 + 2] = b;
	}
}

//float distance(int x1, int y1, int x2, int y2) {
//	return sqrtf((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
//}
#pragma endregion
