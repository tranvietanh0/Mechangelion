#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
// hook-logger.cjs — Utility: structured JSON logging for hook diagnostics
'use strict';
const fs = require('fs');
const path = require('path');

const LOG_DIR = path.join(__dirname, '.logs');
const LOG_FILE = path.join(LOG_DIR, 'hook-log.jsonl');
const MAX_LINES = 1000;
const KEEP_LINES = 500;

function ensureDir() {
  if (!fs.existsSync(LOG_DIR)) fs.mkdirSync(LOG_DIR, { recursive: true });
}

function log(entry) {
  try {
    ensureDir();
    const line = JSON.stringify({ ts: new Date().toISOString(), ...entry }) + '\n';
    fs.appendFileSync(LOG_FILE, line);
    compact();
  } catch {} // never fail
}

function compact() {
  try {
    if (!fs.existsSync(LOG_FILE)) return;
    const content = fs.readFileSync(LOG_FILE, 'utf8');
    const lines = content.trim().split('\n');
    if (lines.length > MAX_LINES) {
      fs.writeFileSync(LOG_FILE, lines.slice(-KEEP_LINES).join('\n') + '\n');
    }
  } catch {} // never fail
}

function read(n = 50) {
  try {
    if (!fs.existsSync(LOG_FILE)) return [];
    const lines = fs.readFileSync(LOG_FILE, 'utf8').trim().split('\n');
    return lines.slice(-n).map(l => { try { return JSON.parse(l); } catch { return null; } }).filter(Boolean);
  } catch { return []; }
}

module.exports = { log, compact, read };
