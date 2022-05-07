export class RequestError extends Error {
    constructor(response) {
      super(response.title);
      this.name = 'RequestError';
  
      this.response = response;
      this.message = response.title;
      this.statusCode = response.status;
    }
  }