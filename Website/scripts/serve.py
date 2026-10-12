"""Strict local preview: directory redirects, actual 404s, no SPA fallback."""
from http.server import ThreadingHTTPServer,SimpleHTTPRequestHandler
from pathlib import Path
from urllib.parse import urlsplit
root=Path(__file__).resolve().parents[1]/'dist'
class Handler(SimpleHTTPRequestHandler):
    def __init__(self,*args,**kwargs): super().__init__(*args,directory=str(root),**kwargs)
    def do_GET(self):
        if urlsplit(self.path).path=='/index.html':
            self.send_response(301);self.send_header('Location','/');self.end_headers();return
        super().do_GET()
    def send_error(self,code,message=None,explain=None):
        if code==404:
            body=(root/'404.html').read_bytes();self.send_response(404);self.send_header('Content-Type','text/html; charset=utf-8');self.send_header('Content-Length',str(len(body)));self.end_headers()
            if self.command!='HEAD':self.wfile.write(body)
        else:super().send_error(code,message,explain)
    def end_headers(self):
        self.send_header('X-Content-Type-Options','nosniff');self.send_header('Referrer-Policy','strict-origin-when-cross-origin');super().end_headers()
    def log_message(self,fmt,*args):pass
print('CASCADE local preview: http://127.0.0.1:4173',flush=True)
ThreadingHTTPServer(('127.0.0.1',4173),Handler).serve_forever()
