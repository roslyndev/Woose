import axios, { AxiosResponse } from 'axios';
import config from '../Config';

const header = {
  "Content-Type": 'application/json',
};

const UploadHeader = {
  "Content-Type": 'multipart/form-data',
};

const ApiHelper = {
  Upload: async (url: string, jsonData?: FormData): Promise<any> => {
    let result:any = {};

    try {
      const response = await axios.post(url, jsonData, { headers: UploadHeader });
      if (response.status === 200) {
        result = response.data;
      } else {
        result.Error('Api Send Fail');
      }
    } catch (e:any) {
      result.Error(e.message);
    }

    return result;
  },

  Post: async (url: string, jsonData?: any): Promise<any> => {
    let result:any = {};

    try {
      console.log("Post : ", url, jsonData);
      const response = await axios.post(url, jsonData, { headers: header });
      if (response.status === 200) {
        result = response.data;
      } else {
        result.Error('Api Send Fail');
      }
    } catch (e:any) {
        result.Error(e.message);
    }

    return result;
  },

  Put: async (url: string, jsonData?: any): Promise<any> => {
    let result:any = {};
  
    try {
      const response = await axios.put(url, jsonData, { headers: header });
      if (response.status === 200) {
        result = response.data;
      } else {
        result.Error('Api Send Fail');
      }
    } catch (e: any) {
      result.Error(e.message);
    }
  
    return result;
  },


  Delete: async (url: string, jsonData?: any): Promise<any> => {
    let result:any = {};
  
    try {
      const response = await axios.delete(url, { data:jsonData, headers: header });
      if (response.status === 200) {
        result = response.data;
      } else {
        result.Error('Api Send Fail');
      }
    } catch (e: any) {
      result.Error(e.message);
    }
  
    return result;
  },

  Get: async (url: string): Promise<any> => {
    let result:any = {};

    try {
      const response = await axios.get(config.api + url, { headers: header });
      if (response.status === 200) {
        result = response.data;
      } else {
        result.Error('Api Send Fail');
      }
    } catch (e:any) {
        result.Error(e.message);
    }

    return result;
  },

  PostSync: (url: string, jsonData: any, callback:Function): any => {
    let result:any = {};

    try {
      axios.post(url, jsonData, { headers: header }).then((rst:AxiosResponse<any,any>) =>{
        if (callback !== null && typeof callback === "function") {
            result.Success(1);
            result.data = rst.data;
            callback(result);
        }
      });
    } catch (e:any) {
        if (callback !== null && typeof callback === "function") {
            result.Error(e.message);
            callback(result);
        }
    }
  },

  GetSync: (url: string, callback:Function): any => {
    let result:any = {};

    try {
      axios.get(url, { headers: header }).then((rst:AxiosResponse<any,any>) => {
        if (callback !== null && typeof callback === "function") {
            result.Success(1);
            result.data = rst.data;
            callback(result);
        }
      });
    } catch (e:any) {
        if (callback !== null && typeof callback === "function") {
            result.Error(e.message);
            callback(result);
        }
    }
  },

  PutSync: (url: string, jsonData: any, callback:Function): any => {
    let result:any = {};

    try {
      axios.put(url, jsonData, { headers: header }).then((rst:AxiosResponse<any,any>) =>{
        if (callback !== null && typeof callback === "function") {
            result.Success(1);
            result.data = rst.data;
            callback(result);
        }
      });
    } catch (e:any) {
        if (callback !== null && typeof callback === "function") {
            result.Error(e.message);
            callback(result);
        }
    }
  },

  DeleteSync: (url: string, jsonData: any, callback:Function): any => {
    let result:any = {};

    try {
      axios.delete(url, { data:jsonData, headers: header }).then((rst:AxiosResponse<any,any>) =>{
        if (callback !== null && typeof callback === "function") {
            result.Success(1);
            result.data = rst.data;
            callback(result);
        }
      });
    } catch (e:any) {
        if (callback !== null && typeof callback === "function") {
            result.Error(e.message);
            callback(result);
        }
    }
  },
};

export default ApiHelper;
