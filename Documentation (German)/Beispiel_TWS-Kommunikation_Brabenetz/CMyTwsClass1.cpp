

#pragma once

// uncomment both plus also in other file for intel compilation, icl or even in ANSI icl /Za 
// which gives the opportunity to run this in high speed intel / ansi environments without any user interface

#define _CRT_SECURE_NO_DEPRECATE 1 // VS 2005 specific, even if you compile ANSI you need it in VS2005 

#include <stdio.h>
#include <string.h>

#include "CMyTwsClass1.h"

//#ifdef USETWS

//#include "CMyTwsClass1.h"

#include "EPosixClientSocket.h"
#include "EPosixClientSocketPlatform.h"

// #include "Contract.h"
// #include "Order.h"

//#define  _AFXDLL		
//#include <afxsock.h> // works with winsock'1', so non 2

const int PING_DEADLINE = 2; // seconds
const int SLEEP_BETWEEN_PINGS = 30; // seconds

const char cancelledStr1[] = "ApiCancelled";
const char cancelledStr2[] = "Cancelled";

const char * string_TickType[] = {
					"BID_SIZE", // 0 bid_size
					"BID", 
					"ASK", 
					"ASK_SIZE", 
					"LAST", 
					"LAST_SIZE",
				"HIGH", 
				"LOW", 
				"VOLUME", 
				"CLOSE",
				"BID_OPTION_COMPUTATION", 
				"ASK_OPTION_COMPUTATION", 
				"LAST_OPTION_COMPUTATION",
				"MODEL_OPTION",
				"OPEN",
				"LOW_13_WEEK",
				"HIGH_13_WEEK",
				"LOW_26_WEEK",
				"HIGH_26_WEEK",
				"LOW_52_WEEK",
				"HIGH_52_WEEK",
				"AVG_VOLUME",
				"OPEN_INTEREST",
				"OPTION_HISTORICAL_VOL",
				"OPTION_IMPLIED_VOL",
				"OPTION_BID_EXCH",
				"OPTION_ASK_EXCH",
				"OPTION_CALL_OPEN_INTEREST",
				"OPTION_PUT_OPEN_INTEREST",
				"OPTION_CALL_VOLUME",
				"OPTION_PUT_VOLUME",
				"INDEX_FUTURE_PREMIUM",
				"BID_EXCH",
				"ASK_EXCH",
				"AUCTION_VOLUME",
				"AUCTION_PRICE",
				"AUCTION_IMBALANCE",
				"MARK_PRICE",
				"BID_EFP_COMPUTATION",
				"ASK_EFP_COMPUTATION",
				"LAST_EFP_COMPUTATION",
				"OPEN_EFP_COMPUTATION",
				"HIGH_EFP_COMPUTATION",
				"LOW_EFP_COMPUTATION",
				"CLOSE_EFP_COMPUTATION",
				"LAST_TIMESTAMP",
				"SHORTABLE",
				"FUNDAMENTAL_RATIOS",
				"RT_VOLUME",
				"HALTED",
				"BID_YIELD",
				"ASK_YIELD",
				"LAST_YIELD",
				"CUST_OPTION_COMPUTATION",
				"NOT_SET"};


///////////////////////////////////////////////////////////
// member funcs
CMyTwsClass1::CMyTwsClass1(char DRIVELETTER, char * DATADIRECTORY, int r_T_instrument)
	: m_pClient(new EPosixClientSocket(this)) // x_pClient(new EPosixClientSocket(this))
	//, m_state(ST_CONNECT)
	, m_sleepDeadline(0)
	, m_orderId(0)
{

DEMO_SPREAD = false; // etremely high spread to get the order filled in any case

USING_MY_MUTEX1 = false; // handles all the concurrent access to our little in memory database

USE_ALSO_CANCEL_ORDER = true;

N_TRADES = 5000; // size of the trades database

T_sym = r_T_instrument;

// My little multiaccess Order Database
v_orderId		= new int [N_TRADES];
v_filled		= new int [N_TRADES];
v_remaining		= new int [N_TRADES];
v_pending_cancel = new int [N_TRADES];
v_cancelled		 = new int [N_TRADES];
v_avgfillprice	= new double [N_TRADES];

v_Limit_price	= new double [N_TRADES];
//TWS_ordered_for_instrument = new int [N_TRADES];

v_order_for_instrument = new int [N_TRADES];
buy_sell = new int [N_TRADES];
to_open_to_close   = new int [N_TRADES];
expected_position  = new int [N_TRADES];
expected_position2 = new int [N_TRADES];

the_message_text = new char * [N_TRADES];


for(n_trades = 0; n_trades < N_TRADES; ++n_trades)
	*(the_message_text+n_trades) = new char[200];

for(n_trades = 0; n_trades < N_TRADES; ++n_trades)
	sprintf(*(the_message_text+n_trades), "No message received from TWS.");



for(n_trades = 0; n_trades < N_TRADES; ++n_trades){
	*(v_orderId + n_trades) = 0;
	*(v_filled + n_trades) = 0;
	*(v_remaining + n_trades) = 0;
	*(v_pending_cancel + n_trades) = 0;
	*(v_cancelled + n_trades) = 0;

	*(v_avgfillprice + n_trades) = 0.0;
	*(v_Limit_price + n_trades)	= 0.0;
	//*(TWS_ordered_for_instrument + n_trades) = 0;

	*(v_order_for_instrument + n_trades) = 0;
	*(buy_sell + n_trades) = 0;
	*(to_open_to_close   + n_trades) = 0;
	*(expected_position  + n_trades) = 0;
	*(expected_position2 + n_trades) = 0;
} // for

N_trades = 0;

m_orderId = -1;

	tws_marketData_ticker_ID = new int [T_sym];

	bid_size = new int [T_sym];
	bid = new double[T_sym];
	ask = new double[T_sym];
	ask_size = new int [T_sym];
	last = new double[T_sym];
	last_size = new int [T_sym];
	volume = new int [T_sym];
	last_timestamp = new int [T_sym];
	p_contract = new Contract[T_sym];
	p_order = new Order[T_sym];

	tracked_expected_position = new int [T_sym];

	order_volume = new int [T_sym];
	lot_size = new int [T_sym];

	for(t_sym = 0; t_sym < T_sym; ++t_sym){

		*(tws_marketData_ticker_ID+t_sym) = -1;

		*(bid_size+t_sym) = -1;
		*(bid+t_sym) = -1.0;
		*(ask+t_sym) = -1.0;
		*(ask_size+t_sym) = -1;

		// and not yet in use but already in here
		*(last+t_sym) = -1.0;
		*(last_size+t_sym) = -1;
		*(volume+t_sym) = -1;
		*(last_timestamp+t_sym) = -1;

		*(tracked_expected_position+t_sym) = 0;
		*(order_volume+t_sym) = 0;
		*(lot_size+t_sym) = 0;
	}			//		for(t_sym = 0; t_sym < T_sym; ++t_sym){

nbase_trades_orderStatus = 0;


	// And now we fill the order 

	// Rollover wird hier eingetragen bzw. durchgeführt

	t_sym = 0;
	if(1){ // muss 0 werden, wenn der ES gar nichtgetradet werden soll
	(*(p_contract+t_sym)).symbol = "ES";
	(*(p_contract+t_sym)).secType = "FUT";
	(*(p_contract+t_sym)).exchange = "GLOBEX";
	(*(p_contract+t_sym)).currency = "USD";
	(*(p_contract+t_sym)).localSymbol = "ESH2"; //"ESZ1"; // ESH2 //"ESU1";// "ESM1";//"ESH1";
	++t_sym;
	}

	if(1){
	(*(p_contract+t_sym)).symbol = "NKD";
	(*(p_contract+t_sym)).secType = "FUT";
	(*(p_contract+t_sym)).exchange = "GLOBEX";
	(*(p_contract+t_sym)).currency = "USD";
	(*(p_contract+t_sym)).localSymbol = "NKDH2"; // "NKDZ1"; // //"NKDU1";//"NKDM1";//"NKDH1"; 
	++t_sym;
	}

	if(0){
	(*(p_contract+t_sym)).symbol = "ZN";
	(*(p_contract+t_sym)).secType = "FUT";
	(*(p_contract+t_sym)).exchange = "ECBOT";
	(*(p_contract+t_sym)).currency = "USD";
	// (*(p_contract+t_sym)).localSymbol = "ZN MAR 12"; // this input does not work , the conID is needed. 
	(*(p_contract+t_sym)).conId = 82284163;
	++t_sym;
	}

//	char s_h[200];
//	FILE * f_h;

sprintf(s_h, "%c:%sOutput\\History.txt", DRIVELETTER, DATADIRECTORY);//, idate); // sSym, idate); 

f_h = fopen(s_h, "w");


s_t = new char * [T_sym];
for(t_sym=0;t_sym<T_sym;++t_sym)
	s_t[t_sym] = new char[200];
f_t = new FILE * [T_sym];

for(t_sym=0;t_sym<T_sym;++t_sym){
	sprintf(s_t[t_sym], "%c:%sOutput\\Trading_%s.txt", DRIVELETTER, DATADIRECTORY, (*(p_contract+t_sym)).symbol.c_str() );//, idate); // sSym, idate); 
	f_t[t_sym] = fopen(s_t[t_sym], "w");
}









//char** v_t;
//v_t = new char * [T_INSTRUMENT];
// x_pClient = new EPosixClientSocket(this); // this seems was not needed for the m_pClient


	//HANDLE hMutex; 

	//hMutex = CreateMutex( NULL, FALSE, (LPCSTR) "MyTWSMutex" ); // Attention L"..." might be needed!


}

CMyTwsClass1::~CMyTwsClass1()
{

}


void CMyTwsClass1::request_numIDS(int numIDs){

	m_pClient->reqIds(numIDs);
	//x_pClient->reqIds(numIDs);

}


bool CMyTwsClass1::connect(const char *host, unsigned int port, int clientId)
{

	// trying to connect
	printf( "Connecting to %s:%d clientId:%d\n", !( host && *host) ? "127.0.0.1" : host, port, clientId);

	bool bRes = m_pClient->eConnect( host, port, clientId);

	if (bRes) {
		printf( "Connected to %s:%d clientId:%d\n", !( host && *host) ? "127.0.0.1" : host, port, clientId);
	}
	else
		printf( "Cannot connect to %s:%d clientId:%d\n", !( host && *host) ? "127.0.0.1" : host, port, clientId);

	return bRes;



}

void CMyTwsClass1::disconnect() const
{
	m_pClient->eDisconnect();

	printf ( "Disconnected\n");
}

bool CMyTwsClass1::isConnected() const
{
	return m_pClient->isConnected();
}

bool CMyTwsClass1::checkMessages()
{

	// if(!m_pClient->isInBufferEmpty()) //...did not work

	// printf("m_pClient->isInBufferEmpty() %d.\n", m_pClient->isInBufferEmpty() ); 
	// check for null archive
	if( m_pClient->m_fd == 0) {
		return false;
	}
	
	return m_pClient->checkMessages();

//m_pClient->reqMktData

	/*

	fd_set readSet, writeSet, errorSet;

	struct timeval tval;
	tval.tv_usec = 0;
	tval.tv_sec = 0;

	time_t now = time(NULL);

	switch (m_state) {
		case ST_PLACEORDER:
			placeOrder();
			break;
		case ST_PLACEORDER_ACK:
			break;
		case ST_CANCELORDER:
			cancelOrder();
			break;
		case ST_CANCELORDER_ACK:
			break;
		case ST_PING:
			reqCurrentTime();
			break;
		case ST_PING_ACK:
			if( m_sleepDeadline < now) {
				disconnect();
				return;
			}
			break;
		case ST_IDLE:
			if( m_sleepDeadline < now) {
				m_state = ST_PING;
				return;
			}
			break;
	}

	if( m_sleepDeadline > 0) {
		// initialize timeout with m_sleepDeadline - now
		tval.tv_sec = m_sleepDeadline - now;
	}

	if( m_pClient->fd() >= 0 ) {

		FD_ZERO( &readSet);
		errorSet = writeSet = readSet;

		FD_SET( m_pClient->fd(), &readSet);

		if( !m_pClient->isOutBufferEmpty())
			FD_SET( m_pClient->fd(), &writeSet);

		FD_CLR( m_pClient->fd(), &errorSet);

		int ret = select( m_pClient->fd() + 1, &readSet, &writeSet, &errorSet, &tval);

		if( ret == 0) { // timeout
			return;
		}

		if( ret < 0) {	// error
			disconnect();
			return;
		}

		if( m_pClient->fd() < 0)
			return;

		if( FD_ISSET( m_pClient->fd(), &errorSet)) {
			// error on socket
			m_pClient->onError();
		}

		if( m_pClient->fd() < 0)
			return;

		if( FD_ISSET( m_pClient->fd(), &writeSet)) {
			// socket is ready for writing
			m_pClient->onSend();
		}

		if( m_pClient->fd() < 0)
			return;

		if( FD_ISSET( m_pClient->fd(), &readSet)) {
			// socket is ready for reading
			m_pClient->onReceive();
		}
	}

*/

}

//////////////////////////////////////////////////////////////////
// methods
void CMyTwsClass1::reqCurrentTime()
{
	printf( "Requesting Current Time\n");

	// set ping deadline to "now + n seconds"
	m_sleepDeadline = time( NULL) + PING_DEADLINE;

	//m_state = ST_PING_ACK;

	m_pClient->reqCurrentTime();
}

void CMyTwsClass1::requestMarketData(){

// requests Market Data for all traded instruments

	if(USING_MY_MUTEX1)
	my_mutex1.lock();

	IBString genericTicks = "100,101,104,105,106,107,165,221,225,233,236,258";

	//void reqMktData(TickerId id, const Contract &contract, const IBString &genericTicks, bool snapshot);
	for(t_sym=0;t_sym<T_sym;++t_sym)
	m_pClient->reqMktData( *(tws_marketData_ticker_ID+t_sym) = m_orderId++ , (*(p_contract+t_sym)), genericTicks, false); // true would not work for snapshot

	ID_BASE = *(tws_marketData_ticker_ID+0);

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

	return;

}			// void requestMarketData(){


int CMyTwsClass1::placeOrder(int * p_tws_order_number, int r_t_sym, int r_buysell, int r_toOpenClose, int r_order_volume, int r_lot_size, double r_limit, int r_expectedPosition, int Buy_to_Signal, int pending_Order_Signal)
{

	printf("\nCMyTwsClass1::placeOrder was called.\n");

	if(USING_MY_MUTEX1){
		my_mutex1.lock();
		printf("Mutex locked.\n");
	}

	//my_mutex1.

	t_sym = r_t_sym;
	buysell = r_buysell;
	limit = r_limit;
	toOpenClose = r_toOpenClose;
	expectedPosition = r_expectedPosition;
	*(order_volume+t_sym)  = r_order_volume;
	*(lot_size+t_sym)  = r_lot_size;

	printf("BidSize %d Bid %.2lf Ask %.2lf AskSize %d, ",				
						*(bid_size+t_sym),
						(double) *(bid+t_sym),
						(double) *(ask+t_sym),
						*(ask_size+t_sym) );

	if(1){
	fprintf(f_t[t_sym], "Sym %d BuySell %d Lim %.3lf toOpenClose %d expectedPosition %d order_volume %d lot_size %d \n",
			t_sym,
			buysell,
			limit,
			toOpenClose,
			expectedPosition, 
			*(order_volume+t_sym), 
			*(lot_size+t_sym) );
	fprintf(f_t[t_sym], "Sym %d BidSize %d Bid %.2lf Ask %.2lf AskSize %d\n",	
						 t_sym,
						*(bid_size+t_sym),
						(double) *(bid+t_sym),
						(double) *(ask+t_sym),
						*(ask_size+t_sym) );
	fflush(f_t[t_sym]);
	} // if(1)


	if(1){ // does not work
	if(buysell > 0){

		(*(p_order+t_sym)).action = "BUY";

		if( *(ask_size+t_sym) == 0 ||  *(ask_size+t_sym) == -1 )
			{
			fprintf(f_t[t_sym], "\n");
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			printf("BUY  *(ask_size+%d) == %d return -1\n", t_sym, *(ask_size+t_sym));
			fprintf(f_t[t_sym], "BUY  *(ask_size+%d) == %d return -1\n", t_sym, *(ask_size+t_sym));
			fflush(f_t[t_sym]);
			return -1;
			} 
		else if( *(ask_size+t_sym) >= (*(order_volume+t_sym)) && *(ask+t_sym) > 0)
			{
			(*(p_order+t_sym)).lmtPrice = *(ask+t_sym);
			(*(p_order+t_sym)).totalQuantity = *(order_volume+t_sym) * *(lot_size+t_sym);
			}
		else if( *(ask_size+t_sym) > 0 && *(ask+t_sym) > 0)
			{
			(*(p_order+t_sym)).lmtPrice = *(ask+t_sym);
			(*(p_order+t_sym)).totalQuantity = *(ask_size+t_sym) * *(lot_size+t_sym);
			}
		else
			{
			fprintf(f_t[t_sym], "\n");
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			printf("Buy else t_sym = %d return -1\n", t_sym);
			fprintf(f_t[t_sym], "Buy else t_sym = %d return -1\n", t_sym);
			fflush(f_t[t_sym]);
			return -1;
			}

		(*(p_order+t_sym)).orderType= "LMT";

	}											// 	if(buysell > 0){							
	else if (buysell < 0){

		(*(p_order+t_sym)).action = "SELL";

		if( *(bid_size+t_sym) == 0 || *(bid_size+t_sym) == -1 )
			{
			fprintf(f_t[t_sym], "\n");
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			printf("SELL  *(bid_size+%d) == %d return -1\n", t_sym, *(bid_size+t_sym));			
			fprintf(f_t[t_sym], "SELL  *(bid_size+%d) == %d return -1\n", t_sym, *(bid_size+t_sym));	
			fflush(f_t[t_sym]);
			return -1;
			}
		else if( *(bid_size+t_sym) >= (*(order_volume+t_sym)) && *(bid+t_sym) > 0)
			{
			(*(p_order+t_sym)).lmtPrice = *(bid+t_sym);
			(*(p_order+t_sym)).totalQuantity = *(order_volume+t_sym) * *(lot_size+t_sym);
			}
		else if( *(bid_size+t_sym) > 0 && *(bid+t_sym) > 0)
			{
			(*(p_order+t_sym)).lmtPrice = *(bid+t_sym);
			(*(p_order+t_sym)).totalQuantity = *(bid_size+t_sym) * *(lot_size+t_sym);
			}
		else 
			{	
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			printf("Sell else t_sym = %d return -1\n", t_sym);
			fprintf(f_t[t_sym], "Sell else t_sym = %d return -1\n", t_sym);
			fflush(f_t[t_sym]);
			return -1;
			}

		(*(p_order+t_sym)).orderType= "LMT";


	} // (buysell < 0){
	else 	// wenn es weder kauf noch verkauf ist bitte retour, aber unlock vorher, wobei dies nie sein sollte
		{	
		* p_tws_order_number = -1;
		if(USING_MY_MUTEX1)
		my_mutex1.unlock();
		printf("buysell > 0 buysell < 0 else t_sym = %d return -1\n", t_sym);
				fprintf(f_t[t_sym], "buysell > 0 buysell < 0 else t_sym = %d return -1\n", t_sym);
		fflush(f_t[t_sym]);

		return -1;
		} 
	} // if(0){ // does not work 




	if(0){
	if(buysell > 0){

		(*(p_order+t_sym)).action = "BUY";

		if( *(ask_size+t_sym) >= (*(order_volume+t_sym)) && *(ask+t_sym) > 0)
			(*(p_order+t_sym)).lmtPrice = *(ask+t_sym);
				// if there is no quote, shown by ask_size == 0 we have various opportunities of acting
		else // various opportunities here, especially dangereous when at night!!!!
		//{(*(p_order+t_sym)).lmtPrice = *(ask+t_sym); fprintf(f_t[t_sym], "In placeOrder it happend in BUYthat *(ask+t_sym) == 0.\n");} // works
		// or retour senden
		//if(0)
		{									// works
			fprintf(f_t[t_sym], "\n");
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			return -1;
		} // else

	} // if(buysell > 0){
	else if(buysell < 0){
		(*(p_order+t_sym)).action = "SELL";

		if( *(bid_size+t_sym) >= (*(order_volume+t_sym)) && *(bid+t_sym) > 0)
			(*(p_order+t_sym)).lmtPrice = *(bid+t_sym);
				// if there is no quote, shown by bid_size == 0 we have various opportunities of acting
		else
		//{(*(p_order+t_sym)).lmtPrice = *(bid+t_sym); fprintf(f_t[t_sym], "In placeOrder it happend in SELL that *(bid+t_sym) == 0.\n");} // works
		// or retour senden
		//if(0)
		{									// works
			fprintf(f_t[t_sym], "\n");
			* p_tws_order_number = -1;
			if(USING_MY_MUTEX1)
			my_mutex1.unlock();
			return -1;
		}
	} // else if(buysell < 0){
	else // wenn es weder kauf noch verkauf ist bitte retour, aber unlock vorher, wobei dies nie sein sollte
	{	
		* p_tws_order_number = -1;
		if(USING_MY_MUTEX1)
		my_mutex1.unlock();
		return -1;
	} //order.action = "";

	(*(p_order+t_sym)).totalQuantity = *(order_volume+t_sym) * *(lot_size+t_sym);
	(*(p_order+t_sym)).orderType= "LMT";
	} // if(0)

	/*
	if(DEMO_SPREAD){

	if(t_sym == 0){
	price_increment = 0.25;
	usual_spread_in_price_increments = 1;
	extra_spread_in_price_increments = 3;

	
	if(DEMO_SPREAD)
	{
	delta_lmt *= 55; limit=1239.50;
	(*(p_order+t_sym)).lmtPrice = limit + delta_lmt; // attention price increments in 0.25 (!)
	}


	}		// t_sym == 0
	else if (t_sym == 1) {
	price_increment = 5.0;
	usual_spread_in_price_increments = 2;
	extra_spread_in_price_increments = 3;

	if(DEMO_SPREAD)
	{
	delta_lmt *= 550;limit = 9755.0;
	(*(p_order+t_sym)).lmtPrice = limit + delta_lmt; // attention, no comma at this instrument and price increments in 5 (!)
	}
	else
	delta_lmt = 0.0;//delta_lmt * price_increment * (usual_spread_in_price_increments + extra_spread_in_price_increments);

	} // t_sym==1
	}// if(DEMO_SPREAD)
	*/

	printf("Placing Order %ld: %s %ld %s at %f\n", m_orderId, (*(p_order+t_sym)).action.c_str(), (*(p_order+t_sym)).totalQuantity, (*(p_contract+t_sym)).symbol.c_str(), (*(p_order+t_sym)).lmtPrice);

	fprintf(f_h,"N_trades %d Placing Order %ld: %s %ld %s at %f\n", N_trades, m_orderId, (*(p_order+t_sym)).action.c_str(), (*(p_order+t_sym)).totalQuantity, (*(p_contract+t_sym)).symbol.c_str(), (*(p_order+t_sym)).lmtPrice);

	fprintf(f_t[t_sym], "N_trades %d Placing Order %ld: %s %ld %s at %f\n", N_trades, m_orderId, (*(p_order+t_sym)).action.c_str(), (*(p_order+t_sym)).totalQuantity, (*(p_contract+t_sym)).symbol.c_str(), (*(p_order+t_sym)).lmtPrice);
	fflush(f_t[t_sym]);

	if(N_trades >= N_TRADES) {
		printf ("N_trades >= N_TRADES, no more orders possible, recompile with bigger N_TRADES.\n"); 
		* p_tws_order_number = -1;
		return -1;
		} // if(N_trades >= N_TRADES) {

	// possible solution for my rt jump problem
	// (*(p_order+t_sym)).totalQuantity
	if((tracked_expected_position[t_sym] + buysell) != expectedPosition){
		printf("(tracked_expected_position[t_sym] + buysell) != expectedPosition, order not accepted.\n");
		fprintf(f_h,"(tracked_expected_position[t_sym] + buysell) != expectedPosition, order not accepted.\n");
		// return -1;
	}

	// p_CMyTwsClass1->v_orderId[t_n_trades = p_CMyTwsClass1->N_trades++] = t_orderId = p_CMyTwsClass1->m_orderId++;	

	v_orderId[N_trades] = m_orderId;
	v_order_for_instrument[N_trades] = t_sym;
	buy_sell[N_trades] = buysell;
	to_open_to_close[N_trades] = toOpenClose;
	expected_position[N_trades] = expectedPosition;
	expected_position2[N_trades] = tracked_expected_position[t_sym] += buysell;
	v_Limit_price[N_trades] = (*(p_order+t_sym)).lmtPrice;

	m_pClient->placeOrder( m_orderId, (*(p_contract+t_sym)), (*(p_order+t_sym)));
	printf("Order placed to m_pClient->placeOrder by placeOrder in CMyTWSClass1.cpp\n");

	*p_tws_order_number = N_trades;

	++m_orderId;
	++N_trades;

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

	return 0;
}

void CMyTwsClass1::cancelOrder(int r_m_orderId)
{
	printf( "Cancelling Order %ld\n", r_m_orderId);
	fprintf(f_h, "Cancelling Order %ld\n", r_m_orderId);

	// m_state = ST_CANCELORDER_ACK;
	//m_pClient->cancelOrder( m_orderId);

	if(USING_MY_MUTEX1)
		my_mutex1.lock();

	m_pClient->cancelOrder( r_m_orderId);

	if(USING_MY_MUTEX1)
		my_mutex1.unlock();

}

///////////////////////////////////////////////////////////////////
// events
void CMyTwsClass1::orderStatus( OrderId r_orderId, const IBString &status, int r_filled,
	   int r_remaining, double r_avgFillPrice, int permId, int parentId,
	   double lastFillPrice, int clientId, const IBString& whyHeld)

{

	if(USING_MY_MUTEX1)
	my_mutex1.lock();

message_orderId = r_orderId;
message_filled = r_filled;
message_remaining = r_remaining;
message_avgFillPrice = r_avgFillPrice;

N_trades_orderStatus = N_trades;


for(n_trades_orderStatus = 0; message_orderId != v_orderId[n_trades_orderStatus]; ++n_trades_orderStatus)	
	;
if(n_trades_orderStatus < N_TRADES && message_orderId == v_orderId[n_trades_orderStatus]){

v_filled[n_trades_orderStatus] = message_filled;

v_remaining[n_trades_orderStatus] = message_remaining;

v_avgfillprice[n_trades_orderStatus] = message_avgFillPrice;

sprintf( *(the_message_text+n_trades_orderStatus), "My orderStatus Order: orderId %d, status %s, filled %d, remaining %d, avgFillPrice %lf, permId %d, parentId %d, lastFillPrice %lf, clientId %d, whyHeld %s.\n",
	   message_orderId, status.c_str(), message_filled, message_remaining, message_avgFillPrice, permId, parentId,
	   lastFillPrice, clientId, whyHeld.c_str() );

//strcmp(
//char cancelledStr1[] = "ApiCancelled";
//char cancelledStr2[] = "Cancelled";

//My orderStatus Order: orderId 112471, status Cancelled, filled 0, remaining 1, avgFillPrice 0.000000, permId 945436123, parentId 0, lastFillPrice 0.000000, clientId 0, whyHeld .

sprintf(receivedStr1, "%s" ,status.c_str());

if( !strncmp(receivedStr1, cancelledStr1, 12) ||
	!strncmp(receivedStr1, cancelledStr2,  9)    )
			v_cancelled[n_trades_orderStatus] = 1;

printf( "My orderStatus Order: orderId %d, status %s, filled %d, remaining %d, avgFillPrice %lf, permId %d, parentId %d, lastFillPrice %lf, clientId %d, whyHeld %s. And strncmp said %d.\n",
	   message_orderId, status.c_str(), message_filled, message_remaining, message_avgFillPrice, permId, parentId,
	   lastFillPrice, clientId, whyHeld.c_str(), v_cancelled[n_trades_orderStatus] );

if(1)
fprintf(f_h, "My orderStatus Order: orderId %d, status %s, filled %d, remaining %d, avgFillPrice %lf, permId %d, parentId %d, lastFillPrice %lf, clientId %d, whyHeld %s. And strncmp said %d.\n",
	   message_orderId, status.c_str(), message_filled, message_remaining, message_avgFillPrice, permId, parentId,
	   lastFillPrice, clientId, whyHeld.c_str(), v_cancelled[n_trades_orderStatus] );



// now lets try a new method to be faster in the desired row of the in-memory database

id_sym_orderStatus = message_orderId - (ID_BASE + T_sym); // funktioniert weil wir die ersten orderIDs für die TickerPreise verwenden

if(n_trades_orderStatus != id_sym_orderStatus){
printf(     "Attention.....................n_trades_orderStatus != id_sym_orderStatus in Order Status!!! %d != %d !!!.\n", n_trades_orderStatus, id_sym_orderStatus);
fprintf(f_h,"Attention.....................n_trades_orderStatus != id_sym_orderStatus in Order Status!!! %d != %d !!!.\n", n_trades_orderStatus, id_sym_orderStatus);
} // if(n_trades_orderStatus != id_sym){





}
else {
	printf("Could not find order in database.\n");
	fprintf(f_h, "Could not find order in database.\n");
}
//for(; v_filled[nbase_trades_orderStatus]==1; ++nbase_trades_orderStatus)
//	;

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

}

void CMyTwsClass1::nextValidId( OrderId orderId)
{

	printf( "My nextValidId event was called with OrderId = %d \n", orderId);
	//fprintf(f_d,"\n

	m_orderId = orderId;
	
	// m_state = ST_PLACEORDER;
}

void CMyTwsClass1::currentTime( long time)
{
	//if ( m_state == ST_PING_ACK) {
	if(1){
		time_t t = ( time_t)time;
		struct tm * timeinfo = localtime ( &t);
		printf( "The current date/time is: %s", asctime( timeinfo));
		//fprintf(f_d,"\n
		time_t now = ::time(NULL);
		m_sleepDeadline = now + SLEEP_BETWEEN_PINGS;

		//m_state = ST_IDLE;
	}
}

void CMyTwsClass1::error(const int id, const int errorCode, const IBString errorString)
{

	printf("My error event was called.\n");

	printf( "Error id=%d, errorCode=%d, msg=%s\n", id, errorCode, errorString.c_str());
	fprintf(f_h,"My error event was called. Error id=%d, errorCode=%d, msg=%s\n", id, errorCode, errorString.c_str());

// when order was cancelled, 
// My error event was called. Error id=112471, errorCode=202, msg=Order Canceled - reason:

	id_sym_error = id - (ID_BASE + T_sym);

	if(errorCode == 202)
		*(v_cancelled+id_sym_error) = 1; // wird in as filled returned aber mit -1 !!!!!

// Too many messages error message, Attention, I think it may have caused a stop of the TWS but I am not sure
//My error event was called. Error id=3, errorCode=100, msg=Max rate of messages per second has been exceeded:max=50 rec=106 (3)

	if( id == -1 && errorCode == 1100) // if "Connectivity between IB and TWS has been lost"
		disconnect();



}

void CMyTwsClass1::tickPrice( TickerId tickerId, TickType field, double r_price, int canAutoExecute) {

// Auto Execute is always 1 when is a bid or ask price and always 0 when is a last prize
	if(USING_MY_MUTEX1)
	my_mutex1.lock();

	id_sym_tickPrice = tickerId - ID_BASE; // this simple method is only for received ticks, not for order ID to buy or sell
	price = r_price;

	if(field == BID)
		*(bid+id_sym_tickPrice) = price;
	else if(field == ASK)
		*(ask+id_sym_tickPrice) = price;
	else if(field == LAST)
		*(last+id_sym_tickPrice) = price;

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

	if(0)
	printf("%d tickPrice event TickType %d %s Price %lf canAutoExecute %d.\n", tickerId, field, string_TickType[field], price, canAutoExecute);
	//fprintf(f_h,"My tickPrice event was called.\n");
}
void CMyTwsClass1::tickSize( TickerId tickerId, TickType field, int r_size) {

	//t_sym ?
	//for( id_sym
	//*(tws_marketData_ticker_ID+t_sym)

	if(USING_MY_MUTEX1)
	my_mutex1.lock();

	id_sym_tickSize = tickerId - ID_BASE;
	size = r_size;

	if(field == BID_SIZE)
		*(bid_size+id_sym_tickSize) = size;
	else if(field == ASK_SIZE)
		*(ask_size+id_sym_tickSize) = size;
	else if(field == LAST_SIZE)
		*(last_size+id_sym_tickSize) = size;

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

	if(0)
	printf("%d tickSize event TickType %d %s size %d.\n", tickerId, field, string_TickType[field], size);
	//fprintf(f_h,"My tickSize event was called.\n");
}
void CMyTwsClass1::tickOptionComputation( TickerId tickerId, TickType tickType, double impliedVol, double delta,
											 double optPrice, double pvDividend,
											 double gamma, double vega, double theta, double undPrice) {
	printf("My tickOptionComputation event was called.\n");
	fprintf(f_h,"My tickOptionComputation event was called.\n");
}
void CMyTwsClass1::tickGeneric(TickerId tickerId, TickType tickType, double value) {
	printf("My tickGeneric event was called.\n");
	fprintf(f_h,"My tickGeneric event was called.\n");
}
void CMyTwsClass1::tickString(TickerId tickerId, TickType tickType, const IBString& value) {

	id_sym_tickString = tickerId - ID_BASE;

	if(tickType == LAST_TIMESTAMP)	// string to int
		*(last_timestamp+id_sym_tickString) = atoi(value.c_str());

	if(0)
	printf("%d tickString event TickType %d %s %s.\n", tickerId, tickType, string_TickType[tickType], value.c_str());
	//fprintf(f_h,"My tickString event was called.\n");
}
void CMyTwsClass1::tickEFP(TickerId tickerId, TickType tickType, double basisPoints, const IBString& formattedBasisPoints,
							   double totalDividends, int holdDays, const IBString& futureExpiry, double dividendImpact, double dividendsToExpiry) {
	printf("My tickEFP event was called.\n");
	fprintf(f_h,"My tickEFP event was called.\n");
}
void CMyTwsClass1::openOrder( OrderId orderId, const Contract&, const Order&, const OrderState& ostate) {
	printf("My openOrder OrderId %d event was called.\n", orderId);
	fprintf(f_h,"My openOrder OrderId %d event was called.\n", orderId);

}
void CMyTwsClass1::openOrderEnd() {

	printf("My openOrderEnd event was called.\n");
	fprintf(f_h,"My openOrderEnd event was called.\n");


}
void CMyTwsClass1::winError( const IBString &str, int lastError) {
	printf("My winError event was called.\n");
	fprintf(f_h,"My winError event was called.\n");
}
void CMyTwsClass1::connectionClosed() {
	printf("My connectionClosed event was called.\n");
	fprintf(f_h,"My connectionClosed event was called.\n");
}
void CMyTwsClass1::updateAccountValue(const IBString& key, const IBString& val,
										  const IBString& currency, const IBString& accountName) {
	printf("My updateAccountValue event was called.\n");
	fprintf(f_h,"My updateAccountValue event was called.\n");
}
void CMyTwsClass1::updatePortfolio(const Contract& contract, int position,
		double marketPrice, double marketValue, double averageCost,
		double unrealizedPNL, double realizedPNL, const IBString& accountName){
	printf("My updatePortfolio event was called.\n");
	fprintf(f_h,"My updatePortfolio event was called.\n");
}
void CMyTwsClass1::updateAccountTime(const IBString& timeStamp) {
	printf("My updateAccountTime event was called.\n");
	fprintf(f_h,"My updateAccountTime event was called.\n");
}
void CMyTwsClass1::accountDownloadEnd(const IBString& accountName) {
	printf("My accountDownloadEnd event was called.\n");
	fprintf(f_h,"My accountDownloadEnd event was called.\n");
}
void CMyTwsClass1::contractDetails( int reqId, const ContractDetails& contractDetails) {
	printf("My contractDetails event was called.\n");
	fprintf(f_h,"My contractDetails event was called.\n");
}
void CMyTwsClass1::bondContractDetails( int reqId, const ContractDetails& contractDetails) {
	printf("My bondContractDetails event was called.\n");
	fprintf(f_h,"My bondContractDetails event was called.\n");
}
void CMyTwsClass1::contractDetailsEnd( int reqId) {
	printf("My contractDetailsEnd event was called.\n");
	fprintf(f_h,"My contractDetailsEnd event was called.\n");
}
void CMyTwsClass1::execDetails( int reqId, const Contract& contract, const Execution& execution) {
	
	printf("My execDetails reqId %d event was called.\n", reqId);
	fprintf(f_h,"My execDetails reqId %d event was called.\n", reqId);
}
void CMyTwsClass1::execDetailsEnd( int reqId) {
	printf("My execDetailsEnd event was called.\n");
	fprintf(f_h,"My execDetailsEnd event was called.\n");
}

void CMyTwsClass1::updateMktDepth(TickerId id, int position, int operation, int side,
									  double price, int size) {
	printf("My updateMktDepth event was called.\n");
	fprintf(f_h,"My updateMktDepth event was called.\n");
}
void CMyTwsClass1::updateMktDepthL2(TickerId id, int position, IBString marketMaker, int operation,
										int side, double price, int size) {
	printf("My updateMktDepthL2 event was called.\n");
	fprintf(f_h,"My updateMktDepthL2 event was called.\n");
}
void CMyTwsClass1::updateNewsBulletin(int msgId, int msgType, const IBString& newsMessage, const IBString& originExch) {
	printf("My updateNewsBulletin event was called.\n");
	fprintf(f_h,"My updateNewsBulletin event was called.\n");
}
void CMyTwsClass1::managedAccounts( const IBString& accountsList) {
	printf("My managedAccounts event was called.\n");
	fprintf(f_h,"My managedAccounts event was called.\n");
}
void CMyTwsClass1::receiveFA(faDataType pFaDataType, const IBString& cxml) {
	printf("My receiveFA event was called.\n");
	fprintf(f_h,"My receiveFA event was called.\n");
}
void CMyTwsClass1::historicalData(TickerId reqId, const IBString& date, double open, double high,
									  double low, double close, int volume, int barCount, double WAP, int hasGaps) {
	printf("My historicalData event was called.\n");
	fprintf(f_h,"My historicalData event was called.\n");
}
void CMyTwsClass1::scannerParameters(const IBString &xml) {
	printf("My scannerParameters event was called.\n");
	fprintf(f_h,"My scannerParameters event was called.\n");
}
void CMyTwsClass1::scannerData(int reqId, int rank, const ContractDetails &contractDetails,
	   const IBString &distance, const IBString &benchmark, const IBString &projection,
	   const IBString &legsStr) {
	printf("My scannerData event was called.\n");
	fprintf(f_h,"My scannerData event was called.\n");
}
void CMyTwsClass1::scannerDataEnd(int reqId) {
	printf("My scannerDataEnd event was called.\n");
	fprintf(f_h,"My scannerDataEnd event was called.\n");
}
void CMyTwsClass1::realtimeBar(TickerId reqId, long time, double open, double high, double low, double close,
								   long volume, double wap, int count) {
	printf("My realtimeBar event was called.\n");
	fprintf(f_h,"My realtimeBar event was called.\n");
}
void CMyTwsClass1::fundamentalData(TickerId reqId, const IBString& data) {
	printf("My fundamentalData event was called.\n");
	fprintf(f_h,"My fundamentalData event was called.\n");
}
void CMyTwsClass1::deltaNeutralValidation(int reqId, const UnderComp& underComp) {
	printf("My deltaNeutralValidation event was called.\n");
	fprintf(f_h," deltaNeutralValidation event was called.\n");
}
void CMyTwsClass1::tickSnapshotEnd(int reqId) {
	printf("My tickSnapshotEnd event was called.\n");
	fprintf(f_h,"My tickSnapshotEnd event was called.\n");
}

int CMyTwsClass1::printDatabase(char DRIVELETTER, char * DATADIRECTORY){

	//char * database_file = "Database";
	char s_d[200];
	FILE * f_d;

sprintf(s_d, "%c:%sOutput\\Database.csv", DRIVELETTER, DATADIRECTORY);//, idate); // sSym, idate); 

f_d = fopen(s_d, "w");

fprintf(f_d, "n_trades;v_orderId;v_filled;v_avgfillprice;v_order_for_instrument;buy_sell;to_open_to_close;expected_position;expected_position2;\n");

for(n_trades = 0; n_trades<N_trades; ++n_trades)
				fprintf(f_d,"%d;%d;%d;%lf;%d;%d;%d;%d;%d;%s;",
									n_trades,
									*(v_orderId+n_trades),
									*(v_filled+n_trades),
									*(v_avgfillprice+n_trades),
									*(v_order_for_instrument+n_trades),
									*(buy_sell+n_trades),
									*(to_open_to_close+n_trades),
									*(expected_position+n_trades),
									*(expected_position2+n_trades),
									*(the_message_text+n_trades)
									);

fclose(f_d);
fclose(f_h);

for(t_sym=0;t_sym<T_sym;++t_sym)
	fclose(f_t[t_sym]);

printf("Database was printed.\n");

return 0;
}


int CMyTwsClass1::isFilled(int t_n_trades, int * isCancelled, int * isFilledalready, int * isRemaining)
{

// Achtung execute signal weiß die Zeile der database in der die order steht während
// die events die TWS Order nummer kennen die jeweils erst wieder in die richtige Orderzeile
// übersetzt werden muß.

// p_CMyTwsClass1->v_filled[t_n_trades]==1)

	if(USING_MY_MUTEX1)
	my_mutex1.lock();

	n_trades_isFilled = t_n_trades;

	if(1)
	if(USE_ALSO_CANCEL_ORDER)
	if(!(v_cancelled[n_trades_isFilled]))
	if(v_remaining[n_trades_isFilled] > 0 && v_pending_cancel[n_trades_isFilled] == 0) // if not jet filled, check if price changed
		if( 
			( buy_sell[n_trades_isFilled] > 0 && v_Limit_price[n_trades_isFilled] <  *(ask+v_order_for_instrument[n_trades_isFilled]) )
			||		
		    ( buy_sell[n_trades_isFilled] < 0 && v_Limit_price[n_trades_isFilled] >  *(bid+v_order_for_instrument[n_trades_isFilled]) ) 
					)
		{
			v_pending_cancel[n_trades_isFilled] = 1;   // a pending cancel will be returned as a not filled until really cancelled
			cancelOrder(v_orderId[n_trades_isFilled]); // cancel Order

			fprintf(f_t[v_order_for_instrument[n_trades_isFilled]], "Cancel %d buy_sell[n_trades_isFilled] %d > 0 && v_Limit_price[n_trades_isFilled] %.2lf <  %.2lf *(ask+v_order_for_instrument[n_trades_isFilled]) || \n",
				n_trades_isFilled,
				buy_sell[n_trades_isFilled], 
				v_Limit_price[n_trades_isFilled],
				*(ask+v_order_for_instrument[n_trades_isFilled]) );

			fprintf(f_t[v_order_for_instrument[n_trades_isFilled]], "Cancel %d buy_sell[n_trades_isFilled] %d < 0 && v_Limit_price[n_trades_isFilled] %.2lf >  %.2lf *(bid+v_order_for_instrument[n_trades_isFilled]) \n",
				n_trades_isFilled, 
				buy_sell[n_trades_isFilled], 
				v_Limit_price[n_trades_isFilled],
				*(bid+v_order_for_instrument[n_trades_isFilled]) );

			fflush(f_t[v_order_for_instrument[n_trades_isFilled]]);
		}
	
	*isFilledalready = v_filled[n_trades_isFilled]; // isFilledalready, 0 wenn nicht filled, 1 wenn filled
	*isRemaining	 = v_remaining[n_trades_isFilled]; 
	*isCancelled	 = v_cancelled[n_trades_isFilled];

	//if(v_cancelled[n_trades_isFilled] == 1)
	//		*isFilledalready = -1;			// -1 wenn bereits cancelled, also schon nach pending cancel!
	
	// *isFilledalready = 0;

	if(USING_MY_MUTEX1)
	my_mutex1.unlock();

// return v_filled[t_n_trades];
return 0;
} // 	int isFilled(int t_orderId);


void CMyTwsClass1::print_message_from_other_classes(char * text_of_message){

printf(text_of_message);
fprintf(f_h, text_of_message);

return;
}




	

