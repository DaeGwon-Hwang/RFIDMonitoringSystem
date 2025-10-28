// JavaScript source code
var SuspendedOperation = new Object();

function TagPrintX_WSC(firedBrowserUnload) {
    this.firedBrowserUnload = firedBrowserUnload;
    var TagPrintXWSS_socket;
    var isWSS_Connected = false;
    var bSuspended = false;
    var lastSendCommand = -1;
    var wssOperationFinished = true;
    var wssCurrentOperationCode = 0;
    var wssCurrentOperationId = 0;
    var wssOnceInitialized = false;
    this.lastErrorCode = -1;
    this.lastErrorMessage;
    this.lastOperationResultMessage;
    this.lastReceiveMessage;
    this.lastReturnMessage;
    this.currentServiceId;
    this.currentFnResult = -1;

    // 요청에 사용되는 유니크 ID를 반환
    getWSSOperationUid = function () {

        return ++wssCurrentOperationId;
    };

    doSendToTagPrintX_WSServer = function (message, cmd) {
        TagPrintXWSS_socket.send(message);
    };

    onSockOpen = function (evt) {
        isWSS_Connected = true;
        wssOnceInitialized = true;
        currentServiceId = "init";
        currentFnResult = 0;
        writeToScreen("TagPrintX_WSS에 연결되었습니다.");

        if (bSuspended == true) {
            if (SuspendedOperation.paramCount == 0) {
                SuspendedOperation.fn();
            }
            else if (SuspendedOperation.paramCount == 1) {
                SuspendedOperation.fn(SuspendedOperation.parameter[0]);
            }
            else if (SuspendedOperation.paramCount == 3) {
                SuspendedOperation.fn(SuspendedOperation.parameter[0], SuspendedOperation.parameter[1], SuspendedOperation.parameter[2]);
            }
            else if (SuspendedOperation.paramCount == 5 && SuspendedOperation.svcName == "print") {
                SuspendedOperation.fn(SuspendedOperation.parameter[0], SuspendedOperation.parameter[1], SuspendedOperation.parameter[2],
                    SuspendedOperation.parameter[3], SuspendedOperation.parameter[4]);
            }
        }
    };


    onSockMessage = function (evt) {

        let receiveData = JSON.parse(evt.data);
        lastReceiveMessage = receiveData;

        let serviceId = receiveData.serviceId;
        let iResult = receiveData.ErrorCode;
        let strResult = receiveData.ErrorMsg;
        currentServiceId = serviceId;
        currentFnResult = receiveData.ErrorCode;

        switch (serviceId) {
            case "SETUP":
                fireOperationEvent(receiveData);
                break;
            case "PRINT":
                firePrintFinishedEvent(receiveData);
                break;
            case "SETFORM":
                break;
            case "SETVARIABLE":
                break;
            case "SELECT_FORM":
                fireOperationEvent(receiveData);
                break;
            case "SELECT_EXCEL":
                fireOperationEvent(receiveData);
                break;
            default:
                break;
        }
    };


    onSockError = function (evt) {
        writeToScreen("ERROR:" + evt.data);
    };


    onSockClose = function (evt) {
        isWSS_Connected = false;
        writeToScreen("TagPrintX_WSS로의 연결이 해지되었습니다.");

        /*
         * 웹소켓 연결이 해제된 경우 자동으로 재연결을 원하면 이곳의 주석을 해제하세요.
        if (firedBrowserUnload == false && wssOnceInitialized == true) {
            TagPrinter.init();
        }
        */
    };


    this.init = function () {
        
        TagPrintXWSS_socket = new WebSocket("wss://localhost:20122");
        currentServiceId = "INIT";
        this.currentFnResult = -1;

        TagPrintXWSS_socket.onopen = onSockOpen;
        TagPrintXWSS_socket.onclose = onSockClose;
        TagPrintXWSS_socket.onmessage = onSockMessage;
        TagPrintXWSS_socket.onerror = onSockError;
    };


    this.doCloseTagPrintXWSS = function () {
        currentServiceId = "CLOSE";
        currentServiceId = 0;
        if (TagPrintXWSS_socket.readyState == WebSocket.OPEN)
            TagPrintXWSS_socket.close();
    };


    this.setup = function (LBL_PRINT_PORT_KND_CD, IP_ADRES, PRINT_PORT_NO) {
        currentServiceId = "SETUP";
        this.currentFnResult = -1;
        if (isWSS_Connected == false) {
            alert("바이텍테크놀로지 태그발행기 서버 프로그램 연결 실패. 다시 연결을 시도합니다.");
            bSuspended = true;
            SuspendedOperation.fn = this.setup;
            SuspendedOperation.svcName = "setup";
            SuspendedOperation.paramCount = 3;
            SuspendedOperation.parameter = [ LBL_PRINT_PORT_KND_CD, IP_ADRES, PRINT_PORT_NO ];
            this.init();

            return;
        }

        if (LBL_PRINT_PORT_KND_CD != "USB" && LBL_PRINT_PORT_KND_CD != "IP") {
            alert("태그발행기의 연결포트 형식이 잘못되었습니다.");
            return;
        }

        if (LBL_PRINT_PORT_KND_CD == "IP" && ipValidateCheck(IP_ADRES) == false) {
            return;
        }

        let requestInfo = new Object();

        requestInfo.serviceId = "SETUP";
        requestInfo.lblPrintPortKndCd = LBL_PRINT_PORT_KND_CD;
        requestInfo.ipAddress = IP_ADRES;
        requestInfo.printPortNo = PRINT_PORT_NO;

        doSendToTagPrintX_WSServer(JSON.stringify(requestInfo), 1);
        bSuspended = false;
    };


    this.setform = function (FORM_FILE_PATH) {
        currentServiceId = "SETFORM";
        this.currentFnResult = -1;
        
        if (isWSS_Connected == false) {
            alert("바이텍테크놀로지 태그발행기 서버 프로그램 연결 실패. 다시 연결을 시도합니다. setform");
            bSuspended = true;
            SuspendedOperation.fn = this.setform;
            SuspendedOperation.svcName = "setform";
            SuspendedOperation.paramCount = 1;
            SuspendedOperation.parameter = [FORM_FILE_PATH];
            this.init();

            return;
        }

        let requestInfo = new Object();
        requestInfo.serviceId = "SETFORM";
        requestInfo.formFileName = FORM_FILE_PATH;

        doSendToTagPrintX_WSServer(JSON.stringify(requestInfo), 5);
        bSuspended = false;
    };


    this.print = function (LBL_PRINT_PORT_KND_CD, IP_ADRES, PRINT_PORT_NO, VAR_DATA, EPC_DATA) {
        currentServiceId = "PRINT";
        this.currentFnResult = -1;
        
        if (isWSS_Connected == false) {
            alert("바이텍테크놀로지 태그발행기 서버 프로그램 연결 실패. 다시 연결을 시도합니다. print");
            bSuspended = true;
            SuspendedOperation.fn = this.print;
            SuspendedOperation.svcName = "print";
            SuspendedOperation.paramCount = 5;
            SuspendedOperation.parameter = [LBL_PRINT_PORT_KND_CD, IP_ADRES, PRINT_PORT_NO, VAR_DATA, EPC_DATA];
            this.init();

            return;
        }

        if (LBL_PRINT_PORT_KND_CD != "USB" && LBL_PRINT_PORT_KND_CD != "IP") {
            alert("태그발행기의 연결포트 형식이 잘못되었습니다.");
            return;
        }

        if (LBL_PRINT_PORT_KND_CD == "IP" && ipValidateCheck(IP_ADRES) == false) {
            return;
        }

        let requestInfo = new Object();
        requestInfo.serviceId = "PRINT";
        requestInfo.lblPrintPortKndCd = LBL_PRINT_PORT_KND_CD;
        requestInfo.ipAddress = IP_ADRES;
        requestInfo.printPortNo = PRINT_PORT_NO;
        requestInfo.varData = VAR_DATA;
        requestInfo.epcData = EPC_DATA;
        doSendToTagPrintX_WSServer(JSON.stringify(requestInfo), 4);
        bSuspended = false;
    };

    this.select_form = function () {
        if (isWSS_Connected == false) {
            alert("바이텍테크놀로지 태그발행기 서버 프로그램 연결 실패. 다시 연결을 시도합니다.");
            bSuspended = true;
            SuspendedOperation.fn = this.select_form;
            SuspendedOperation.svcName = "select_form";
            SuspendedOperation.paramCount = 0;
            this.init();

            return;
        }

        let requestInfo = new Object();

        requestInfo.serviceId = "SELECT_FORM";

        doSendToTagPrintX_WSServer(JSON.stringify(requestInfo), 2);
        bSuspended = false;
    };

    this.select_excel = function () {
        if (isWSS_Connected == false) {
            alert("바이텍테크놀로지 태그발행기 서버 프로그램 연결 실패. 다시 연결을 시도합니다.");
            bSuspended = true;
            SuspendedOperation.fn = this.select_excel;
            SuspendedOperation.svcName = "select_excel";
            SuspendedOperation.paramCount = 0;
            this.init();

            return;
        }

        let requestInfo = new Object();

        requestInfo.serviceId = "SELECT_EXCEL";

        doSendToTagPrintX_WSServer(JSON.stringify(requestInfo), 3);
        bSuspended = false;
    };

    // 태그발행을 제외한 처리완료 메시지가 수신되면 결과처리를 위해 "onOperationCompleted" 이벤트를 발생시킨다.
    fireOperationEvent = function (receiveData) {
        let customEvent;

        lastErrorCode = receiveData.ErrorCode;
        lastErrorMessage = receiveData.ErrorMsg;
        lastReturnMessage = receiveData.ReturnMsg;
        currentFnResult = lastErrorCode;

        if (document.createEvent) {
            customEvent = document.createEvent("HTMLEvents");
            customEvent.initEvent("onOperationCompleted", true, true);
            window.dispatchEvent(customEvent);
        }
        else {
            customEvent = document.createEventObject();
            customEvent.eventType = "onOperationCompleted";
            window.fireEvent("onOperationCompleted", customEvent);
        }

    };



    // 태그발행완료 메시지가 수신되면 결과처리를 위해 "onPrintCompleted" 이벤트를 발생시킨다.
    firePrintFinishedEvent = function (receiveData) {
        let customEvent;
        lastErrorCode = receiveData.ErrorCode;
        lastErrorMessage = receiveData.ErrorMsg;
        lastReturnMessage = receiveData.ReturnMsg;
        currentFnResult = lastErrorCode;
        currentServiceId = receiveData.serviceId;

        if (document.createEvent) {
            customEvent = document.createEvent("HTMLEvents");
            customEvent.initEvent("onPrintCompleted", true, true);
            window.dispatchEvent(customEvent);
        }
        else {
            customEvent = document.createEventObject();
            customEvent.eventType = "onPrintCompleted";
            window.fireEvent("onPrintCompleted", customEvent);
        }
    };

    this.setFiredBrowserUnload = function (_firedBrowserUnload) {
        firedBrowserUnload = _firedBrowserUnload;
    };

    this.getFiredBrowserUnload = function () {
        return firedBrowserUnload;
    };

    this.getLastErrorCode = function () {
        return lastErrorCode;
    };

    this.getLastErrorMessage = function () {
        return lastErrorMessage;
    };

    this.getLastReceivedMessage = function () {
        return lastReceiveMessage;
    };

    this.getLastErrorCount = function () {
        return lastReturnMessage;
    };

    this.getCurrentServiceId = function () {
        return currentServiceId;
    };

    this.getCurrentFnResult = function () {
        return this.currentFnResult;
    };
}


// IP주소의 유효성을 체크한다.
function ipValidateCheck(ipAddr) {
    var f = document.form1;
    var dot_cnt = 0;

    if (isNaN(ipAddr) == false) {
        alert("정확한 아이피를 입력하십시오");
        return false;
    }

    dot_cnt = ipAddr.split(".");
    if (dot_cnt.length < 4) {
        alert("사용하는 아이피를 정확하게 입력하십시오");
        return false;
    }
    for (var i = 0; i < dot_cnt.length; i++) {
        if (isNaN(dot_cnt[i]) == true) {
            alert("아이피는 숫자로 입력하셔야 합니다.");
            return false;
        }
        if ((parseInt(dot_cnt[i]) > 256) || (parseInt(dot_cnt[i]) < 0)) {
            alert("아이피는 1~255사이의 숫자를 입력하십시오");
            return false;
        }
    }

    return true;
}