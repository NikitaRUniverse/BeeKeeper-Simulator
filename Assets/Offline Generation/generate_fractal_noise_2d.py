import numpy as np
from PIL import Image

# Generate 2D fractal noise by combining multiple layers of Perlin noise.

def generate_fractal_noise_2d(shape, res, octaves=4, persistence=0.5, lacunarity=2.0):
    def generate_tileable_perlin_noise_2d(shape, res):
        delta = (res[0] / shape[0], res[1] / shape[1])
        d = (shape[0] // res[0], shape[1] // res[1])
        grid = np.random.rand(res[0] + 1, res[1] + 1)
        angles = grid * 2 * np.pi
        gradients = np.stack((np.cos(angles), np.sin(angles)), axis=2)

        # Generate a random grid of angles and compute gradients
        def interpolate(t):
            return t * t * (3 - 2 * t)

        # Compute Perlin noise values for given coordinates.
        def perlin(x, y):
            xi, xf = np.floor(x).astype(int), x - np.floor(x)
            yi, yf = np.floor(y).astype(int), y - np.floor(y)

            xi0, xi1 = xi % res[0], (xi + 1) % res[0]
            yi0, yi1 = yi % res[1], (yi + 1) % res[1]

            g00 = gradients[xi0, yi0]
            g10 = gradients[xi1, yi0]
            g01 = gradients[xi0, yi1]
            g11 = gradients[xi1, yi1]

            dot00 = np.sum(g00 * np.stack((xf, yf), axis=-1), axis=-1)
            dot10 = np.sum(g10 * np.stack((xf - 1, yf), axis=-1), axis=-1)
            dot01 = np.sum(g01 * np.stack((xf, yf - 1), axis=-1), axis=-1)
            dot11 = np.sum(g11 * np.stack((xf - 1, yf - 1), axis=-1), axis=-1)

            u, v = interpolate(xf), interpolate(yf)
            return dot00 * (1 - u) * (1 - v) + dot10 * u * (1 - v) + dot01 * (1 - u) * v + dot11 * u * v

        nx, ny = np.linspace(0, res[0], shape[0], endpoint=False), np.linspace(0, res[1], shape[1], endpoint=False)
        x, y = np.meshgrid(nx, ny)
        return perlin(x, y)

    # Initialize the noise array and parameters for octaves
    noise = np.zeros(shape, dtype=np.float32)
    frequency = 1
    amplitude = 1
    total_amplitude = 0

    # Loop over each octave, adding scaled Perlin noise
    for _ in range(octaves):
        noise += amplitude * generate_tileable_perlin_noise_2d(shape, (int(res[0] * frequency), int(res[1] * frequency)))
        total_amplitude += amplitude
        amplitude *= persistence
        frequency *= lacunarity

    # Normalize the noise to the range [0, 1]
    return noise / total_amplitude

# Generate a cloud-like texture using fractal noise.

def generate_cloud_texture(resolution):
    shape = (resolution, resolution)
    res = (4, 4)
    noise = generate_fractal_noise_2d(shape, res)
    noise_normalized = ((noise - noise.min()) / (noise.max() - noise.min()) * 255).astype(np.uint8)
    return Image.fromarray(noise_normalized)

# Save a PIL Image object as a TIFF file.

def save_as_tiff(image, filename):
    image.save(filename, format="TIFF")

if __name__ == "__main__":
    resolution = 512
    clouds_texture = generate_cloud_texture(resolution)
    save_as_tiff(clouds_texture, "clouds_texture_1.tiff")
    clouds_texture = generate_cloud_texture(resolution)
    save_as_tiff(clouds_texture, "clouds_texture_2.tiff")
