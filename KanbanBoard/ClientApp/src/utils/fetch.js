export async function post(url = '', data = {}) {
  return await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
      //'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: JSON.stringify(data)
    //body: new URLSearchParams(data)
  });
}

export async function postj(url = '', data = {}) {
  const response = await post(url, data);
  if (!response.ok) {
    console.log('Request execution error\n', response);
    if (response.headers.get('content-type')?.includes('application/problem+json') || false)
      throw new RequestError(response.json());
    throw new Error('Request execution error');
  } 
  return await response.json();
}

export async function get(url = '', data = {}) {
  const response = await fetch(url + '?' + new URLSearchParams(data), { method: 'GET' });
  return await response.json();
}

export async function getj(url = '', data = {}) { return await (await get(url, data)).json(); }