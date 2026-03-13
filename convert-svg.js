const sharp = require('sharp');
const pngToIco = require('png-to-ico');
const fs = require('fs');
const path = require('path');

const baseDir = "C:\\Users\\PC\\Desktop\\demo";
const iconsDir = path.join(baseDir, 'Icons');
const outputDir = baseDir;

// 确保输出目录存在
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

// 读取 Icons 目录下的所有 SVG 文件
const svgFiles = fs.readdirSync(iconsDir).filter(f => f.endsWith('.svg'));

async function convertSvg(inputPath, outputName) {
    const baseName = path.basename(inputPath, '.svg');
    
    // 读取 SVG 文件
    const svgBuffer = fs.readFileSync(inputPath);
    
    // 获取 SVG 的原始尺寸
    const metadata = await sharp(svgBuffer).metadata();
    console.log(`Processing ${baseName}: ${metadata.width}x${metadata.height}`);
    
    // 生成 16x16 PNG
    const png16Path = path.join(outputDir, `${baseName}_16.png`);
    await sharp(svgBuffer)
        .resize(16, 16)
        .png()
        .toFile(png16Path);
    console.log(`Created: ${png16Path}`);
    
    // 生成 32x32 PNG
    const png32Path = path.join(outputDir, `${baseName}_32.png`);
    await sharp(svgBuffer)
        .resize(32, 32)
        .png()
        .toFile(png32Path);
    console.log(`Created: ${png32Path}`);
    
    // 生成 ICO (包含 16x16 和 32x32)
    const icoPath = path.join(outputDir, `${baseName}.ico`);
    const pngBuffers = [
        await sharp(svgBuffer).resize(16, 16).png().toBuffer(),
        await sharp(svgBuffer).resize(32, 32).png().toBuffer()
    ];
    const icoBuffer = await pngToIco(pngBuffers);
    fs.writeFileSync(icoPath, icoBuffer);
    console.log(`Created: ${icoPath}`);
}

async function main() {
    console.log('Converting SVG files to PNG and ICO...\n');
    
    for (const svgFile of svgFiles) {
        const inputPath = path.join(iconsDir, svgFile);
        try {
            await convertSvg(inputPath, svgFile);
        } catch (err) {
            console.error(`Error converting ${svgFile}:`, err.message);
        }
    }
    
    console.log('\nDone!');
}

main();
