declare global {
    interface Window {
        daum: any;
    }
}

const Utility = {
    formatDate : function (date:any, str:string):string {
        if (typeof date === "string") {
            date = new Date(date);
        } else {
            date = date as Date;
        }

        if (date instanceof Date) {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, "0");
            const day = String(date.getDate()).padStart(2, "0");
            const hours = String(date.getHours()).padStart(2, "0");
            const minutes = String(date.getMinutes()).padStart(2, "0");

            return str.replaceAll('yyyy',String(year)).replaceAll('yy',Utility.Right(String(year),2)).replaceAll('MM',month).replaceAll('dd',day).replaceAll('HH',hours).replaceAll('hh',hours).replaceAll('mm',minutes);
        } else {
            return date;
        }
    },
    Right: function(str:string, length:number) {
        if (typeof str !== 'string') {
            return '';
        }
        if (str.length <= length) {
            return str;
        } else {
            return str.slice(-length);
        }
    },
    Left: function(str:string, length:number) {
        if (typeof str !== 'string') {
            return '';
        }
        if (str.length <= length) {
            return str;
        } else {
            return str.slice(0, length);
        }
    },
    FindAddr: function(callback:Function) {
        try {
            new window.daum.Postcode({
                oncomplete:function(data:any) {
                    callback(data);
                }
            }).open();
        } catch (e:any) {
            console.log('daum address api error : ', e);
        }
    },
    WinOpen: function(url:string, option:string, callback?:Function) {
        const printWindow = window.open(url, '_blank', option);
        if (printWindow) {
            if (callback !== null && callback !== undefined && typeof callback === "function") {
                callback(printWindow);
            }
        }
    },
    Popup: function(width:number, height:number, htmlContent:string) {
        let targetID = "PopupLayer";
        if (document.getElementById(targetID) !== null && document.getElementById(targetID) !== undefined) {
            document.getElementById(targetID)?.remove();
        }
        let popup = document.createElement("div") as HTMLDivElement;
        popup.id = targetID;
        popup.className = "PopupLayerParent";

        let popupContent = document.createElement("div") as HTMLDivElement;
        popupContent.className = "PopupLayerContent";
        popupContent.style.width = `${width}px`;
        popupContent.style.height = `${height}px`;
        popupContent.style.top = `calc(50vh - ${height / 2}px)`;
        popupContent.style.left = `calc(50vw - ${width / 2}px)`;
        popupContent.innerHTML = htmlContent;

        let popupButton = document.createElement("button") as HTMLButtonElement;
        popupButton.className = "PopupLayerButton";
        popupButton.innerHTML = `<i class="fa-solid fa-xmark"></i>`;
        popupButton.style.top = `calc(50vh - ${(height / 2) + 10}px)`;
        popupButton.style.left = `calc(50vw + ${(width / 2) - 12}px)`;
        popupButton.addEventListener('click', () => {
            let currnettargetID = "PopupLayer";
            if (document.getElementById(currnettargetID) !== null && document.getElementById(currnettargetID) !== undefined) {
                document.getElementById(currnettargetID)?.remove();
            }
        });

        popup.appendChild(popupContent);
        popup.appendChild(popupButton);
        document.body.appendChild(popup);
    },
    isMobile:function() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    },
    getRandomElements:function(array:any[], numElements:number):any[] {
        // 배열 복사
        const newArray = [...array];
        const result = [] as any[];

        // 배열의 길이가 numElements보다 작으면 전체 배열을 반환
        if (array.length <= numElements) {
            return newArray;
        }

        for (let i = 0; i < numElements; i++) {
            // 남아 있는 요소 중 랜덤으로 하나 선택
            const randomIndex = Math.floor(Math.random() * newArray.length);
            // 선택된 요소를 결과 배열에 추가하고, 복사한 배열에서 제거
            result.push(newArray.splice(randomIndex, 1)[0]);
        }

        return result;
    },
    generateRandomString:function (length:number):string {
        const characters = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
        let randomString = '';
        
        for (let i = 0; i < length; i++) {
            const randomIndex = Math.floor(Math.random() * characters.length);
            randomString += characters.charAt(randomIndex);
        }
        
        return randomString;
    },
    formatTimeAgo:function(date:Date):string {
        // 만약 date가 문자열이라면 Date 객체로 변환
        if (!(date instanceof Date)) {
            date = new Date(date);
        }

        const now = new Date();
        const diffInMilliseconds = now.getTime() - date.getTime();

        // 밀리초 단위로 시간 차이 계산
        const minute = 60 * 1000;
        const hour = 60 * minute;
        const day = 24 * hour;

        if (diffInMilliseconds < minute) {
            // 1분 미만일 경우 "just now"
            return "지금";
        } else if (diffInMilliseconds < hour) {
            // 1시간 미만일 경우 "x minutes ago"
            const minutesAgo = Math.floor(diffInMilliseconds / minute);
            return `${minutesAgo}분전`;
        } else if (diffInMilliseconds < day) {
            // 1시간 이상일 경우 "x hours ago"
            const hoursAgo = Math.floor(diffInMilliseconds / hour);
            return `${hoursAgo}시간전`;
        } else {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, "0");
            const day = String(date.getDate()).padStart(2, "0");
            return `${year}.${(month.length > 1) ? month : '0' + month}.${(day.length > 1) ? day : '0' + day}`;
        }
    },
    IsNullOrEmpty : function(str?:string|undefined|null):boolean {
        let result:boolean = true;

        try {
            if (str === null || str === undefined) {
                result = true;
            } else {
                if (str.trim() === "") {
                    result = true;
                } else {
                    result = false;
                }
            }
        } catch (e) {
            result = true;
        }

        return result;
    },
    extractImageSrc : function(htmlContent:string):(string|null) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlContent, 'text/html');
        const imgElements = doc.querySelectorAll('img');
        const srcList = Array.from(imgElements).map((img) => img.getAttribute('src'));
        return (srcList !== null && srcList !== undefined && srcList.length > 0) ? srcList[0] : null;
    },
    adjustYoutubeIframeSize : function (content:string) {
        const iframeRegex = /<iframe.*?<\/iframe>/;
        const iframeTag = content.match(iframeRegex);

        if (iframeTag) {
          const widthRegex = /width=["'](\d+)["']/;
          const heightRegex = /height=["'](\d+)["']/;
          const srcRegex = /src=["']([^"']+)["']/;
      
          const widthMatch = iframeTag[0].match(widthRegex);
          const heightMatch = iframeTag[0].match(heightRegex);
          const srcMatch = iframeTag[0].match(srcRegex);
      
          const width = widthMatch ? parseInt(widthMatch[1]) : null;
          const height = heightMatch ? parseInt(heightMatch[1]) : null;
          const src = srcMatch ? srcMatch[1] : null;
      
          if (width !== null && height !== null && src !== null) {
            

            // 모바일 화면에 맞게 비율 조절
            const mobileWidth = Math.min(width, window.innerWidth);
            const mobileHeight = (mobileWidth / width) * height;
      
            // 새로운 iframe 태그 조합
            const newIframeTag = `<iframe width="${mobileWidth}" height="${mobileHeight}" src="${src}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen=""></iframe>`;
      
            // 원본 내용에서 기존 iframe 태그를 새로 조합한 태그로 교체
            const modifiedContent = content.replace(iframeRegex, newIframeTag);
            return modifiedContent;
          }
        }
      
        return content;
    },
    addComma:function(number:number):string {
        return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    },
    DeepCopy:function(obj:object):object {
        return JSON.parse(JSON.stringify(obj));
    },
    ShallowCopy:function(obj:object):object {
        return Object.assign({}, obj);
    }
};

export default Utility;