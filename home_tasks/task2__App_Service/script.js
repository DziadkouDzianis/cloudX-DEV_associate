import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 50 },
    { duration: '3m20s', target: 50 },
    { duration: '1m0s', target: 0 },
  ],
};

export default function () {
	for (let id = 1; id <= 12; id++) {
	const res = http.get(`http://public-api-us-ddziadkou-vsp.azurewebsites.net/api/catalog-items/${id}`);
	check(res, { 'status was 200': (r) => r.status == 200 });
	sleep(0.1)
		}
}