const DbMsg = (msg:string):string => {
    return "db." + msg.trim().toLowerCase().replaceAll(' ', '_');
};

export default DbMsg;