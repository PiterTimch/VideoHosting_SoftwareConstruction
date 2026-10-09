import http from 'k6/http';
import { check, sleep } from 'k6';
import { FormData } from 'https://jslib.k6.io/formdata/0.0.2/index.js';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

export let options = {
  stages: [
    { duration: '10s', target: 20 },
    { duration: '20s', target: 20 },
    { duration: '10s', target: 0 },
  ],
  thresholds: {
    // 80% must finish within 2000ms
    'http_req_duration{type:upload}': ['p(80)<2000'],
    'http_req_duration{type:search}': ['p(80)<2000'],
    'http_req_duration{type:sort}': ['p(80)<2000'],
  },
};

const BASE_URL = 'http://localhost:8080/api';

export default function () {
  // We randomly assign users to different tasks
  const task = randomIntBetween(1, 3);

  if (task === 1) {
    // 1. Parallel Uploading Task
    let fd = new FormData();
    const fileBin = open('./fixtures/sample_video.mp4', 'b');
    fd.append('file', http.file(fileBin, 'video.mp4', 'video/mp4'));
    fd.append('title', 'Load Test Video ' + __ITER);
    
    let uploadRes = http.post(`${BASE_URL}/Videos/Upload`, fd.body(), {
      headers: { 'Content-Type': 'multipart/form-data; boundary=' + fd.boundary },
      tags: { type: 'upload' }
    });
    
    check(uploadRes, { 'upload finished': (r) => r.status !== 500 });
  } 
  else if (task === 2) {
    // 2. Random Searching
    const queries = ['tutorial', 'gameplay', 'vlog', 'music'];
    const q = queries[randomIntBetween(0, queries.length - 1)];
    let searchRes = http.get(`${BASE_URL}/Videos/Search?query=${q}`, { tags: { type: 'search' } });
    
    check(searchRes, { 'search finished': (r) => r.status !== 500 });
  } 
  else {
    // 3. Sorting / Fetching lists
    const sorts = ['date', 'views', 'likes'];
    const s = sorts[randomIntBetween(0, sorts.length - 1)];
    let sortRes = http.get(`${BASE_URL}/Videos/GetVideos?sortBy=${s}`, { tags: { type: 'sort' } });
    
    check(sortRes, { 'sort finished': (r) => r.status !== 500 });
  }

  sleep(1);
}

export function handleSummary(data) {
  let passed = 0;
  let failed = 0;
  
  // Calculate passes and fails specifically based on threshold success logic
  data.root_group.checks.forEach(c => {
    passed += c.passes;
    failed += c.fails;
  });
  
  const total = passed + failed;
  return {
    'stdout': `\nTotal tests: ${total}. Passed: ${passed}. Failed: ${failed}.\n`
  };
}
