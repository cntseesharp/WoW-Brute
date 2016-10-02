/*
 * Copyright ¸ 2005 Kele (fooleau@gmail.com)
 * This library is free software; you can redistribute it and/or 
 * modify it under the terms of the GNU Lesser General Public 
 * License version 2.1 as published by the Free Software Foundation
 * (the "LGPL").
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY
 * OF ANY KIND, either express or implied.
 */

using System;
	/// <summary>
	/// Realmlist OpCodes
	/// </summary>
	public enum RLOp
	{
		AUTH_LOGON_CHALLENGE = 0x00,
		AUTH_LOGON_PROOF = 0x01,
		AUTH_RECONNECT_CHALLENGE = 0x02,
		AUTH_RECONNECT_PROOF = 0x03,
		REALM_LIST = 0x10,
        SURVEY = 0x30,
	}

    enum LogonOpCodes
    {
        LOGIN_OK = 0x00,
        LOGIN_FAILED = 0x01,
        LOGIN_FAILED2 = 0x02,
        LOGIN_BANNED = 0x03,
        LOGIN_UNKNOWN_ACCOUNT = 0x04,
        LOGIN_UNKNOWN_ACCOUNT3 = 0x05,
        LOGIN_ALREADYONLINE = 0x06,
        LOGIN_NOTIME = 0x07,
        LOGIN_DBBUSY = 0x08,
        LOGIN_BADVERSION = 0x09,
        LOGIN_DOWNLOAD_FILE = 0x0A,
        LOGIN_FAILED3 = 0x0B,
        LOGIN_SUSPENDED = 0x0C,
        LOGIN_FAILED4 = 0x0D,
        LOGIN_CONNECTED = 0x0E,
        LOGIN_PARENTALCONTROL = 0x0F,
        LOGIN_LOCKED_ENFORCED = 0x10
    }
    

    enum WorldType
    {
        PVE = 0x00,
        PVP = 0x01,
        RP = 0x06,
    }


    public enum LogType
    {
        Error,              // Error Message. (This should be saved for fatal messages?)
        System,             // System Message
        SystemDebug,        // System Debug Message
        NeworkComms,        // Network Communications
        FileDebug           // File reading debug info
    }

    public struct Realm
    {
        public UInt32 Type;
        public byte Color;
        public byte NameLen;
        public string Name;
        public byte AddrLen;
        public string Address;
        public float Population;
        public byte NumChars;
        public byte Language;
        public byte Unk; // const: 1
    }