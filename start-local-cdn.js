// @ts-nocheck
/**
 * SaveWorld 本地 CDN 服务器
 *
 * 用途: 打包后启动，为微信开发者工具提供本地资源下载服务
 * 用法: node start-local-cdn.js [port]
 *
 * 默认端口: 18765
 * 服务目录: Builds/WebGL/webgl/
 *
 * 启动后修改 minigame/game.js 中 DATA_CDN 为 http://localhost:18765
 */
const http = require('http');
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const PORT = parseInt(process.argv[2] || '18765', 10);
const SERVE_DIR = path.resolve(__dirname, 'Builds/webgl');

const MIME_TYPES = {
    '.txt': 'text/plain',
    '.bin': 'application/octet-stream',
    '.data': 'application/octet-stream',
    '.js': 'application/javascript',
    '.wasm': 'application/wasm',
    '.br': 'application/brotli',
    '.json': 'application/json',
    '.bundle': 'application/octet-stream',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
};

// 大于此大小的文件才压缩（避免小文件压缩反而更慢）
const COMPRESS_MIN_SIZE = 1024; // 1KB

// 需要压缩的文件后缀
const COMPRESSIBLE_EXTENSIONS = new Set([
    '.txt', '.js', '.json', '.css', '.html', '.xml', '.svg',
    '.wasm', '.data', '.bin', '.bundle',
]);

const server = http.createServer((req, res) => {
    // 允许跨域（微信开发者工具需要）
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, HEAD, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', '*');
    res.setHeader('Vary', 'Accept-Encoding');

    if (req.method === 'OPTIONS') {
        res.writeHead(204);
        res.end();
        return;
    }

    // 解码路径，去除查询参数
    const urlPath = decodeURIComponent(req.url.split('?')[0]);
    const filePath = path.join(SERVE_DIR, urlPath.replace(/^\//, ''));

    // 安全检查：防止路径穿越
    if (!filePath.startsWith(SERVE_DIR)) {
        res.writeHead(403);
        res.end('Forbidden');
        return;
    }

    fs.stat(filePath, (err, stat) => {
        if (err || !stat.isFile()) {
            console.log(`[404] ${urlPath}`);
            res.writeHead(404);
            res.end('Not Found');
            return;
        }

        const ext = path.extname(filePath);
        const mimeType = MIME_TYPES[ext] || 'application/octet-stream';
        const totalSize = stat.size;
        const sizeMB = (totalSize / 1024 / 1024).toFixed(1);

        // 判断客户端是否支持压缩
        const acceptEncoding = req.headers['accept-encoding'] || '';
        const useBr = acceptEncoding.includes('br');
        const useGzip = acceptEncoding.includes('gzip');
        const shouldCompress = (useBr || useGzip)
            && totalSize >= COMPRESS_MIN_SIZE
            && COMPRESSIBLE_EXTENSIONS.has(ext);

        // 支持 Range 请求（断点续传）— 压缩和 Range 不共存
        const range = req.headers.range;

        if (range && !shouldCompress) {
            const parts = range.replace(/bytes=/, '').split('-');
            const start = parseInt(parts[0], 10);
            const end = parts[1] ? parseInt(parts[1], 10) : totalSize - 1;
            const chunkSize = end - start + 1;

            res.writeHead(206, {
                'Content-Type': mimeType,
                'Content-Length': chunkSize,
                'Content-Range': `bytes ${start}-${end}/${totalSize}`,
                'Accept-Ranges': 'bytes',
            });

            fs.createReadStream(filePath, { start, end }).pipe(res);
            console.log(`[206] ${urlPath} (${sizeMB} MB) range=${start}-${end}`);
        } else if (shouldCompress) {
            // 优先 brotli，其次 gzip
            const encoding = useBr ? 'br' : 'gzip';
            const compressStream = useBr
                ? zlib.createBrotliCompress({ params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 4 } })
                : zlib.createGzip({ level: 6 });

            res.writeHead(200, {
                'Content-Type': mimeType,
                'Content-Encoding': encoding,
                'Accept-Ranges': 'bytes',
                'Cache-Control': 'no-cache',
            });

            fs.createReadStream(filePath).pipe(compressStream).pipe(res);
            console.log(`[200] ${urlPath} (${sizeMB} MB) [${encoding}]`);
        } else {
            res.writeHead(200, {
                'Content-Type': mimeType,
                'Content-Length': totalSize,
                'Accept-Ranges': 'bytes',
                'Cache-Control': 'no-cache',
            });
            fs.createReadStream(filePath).pipe(res);
            console.log(`[200] ${urlPath} (${sizeMB} MB)`);
        }
    });
});

server.listen(PORT, '0.0.0.0', () => {
    console.log('');
    console.log('========================================');
    console.log(`  SaveWorld Local CDN Server`);
    console.log(`  Port: ${PORT}`);
    console.log(`  Dir:  ${SERVE_DIR}`);
    console.log(`  URL:  http://localhost:${PORT}`);
    console.log(`  Compression: gzip / brotli`);
    console.log('========================================');
    console.log('');
    console.log(`请在 minigame/game.js 中设置:`);
    console.log(`  DATA_CDN: 'http://localhost:${PORT}'`);
    console.log('');
});
