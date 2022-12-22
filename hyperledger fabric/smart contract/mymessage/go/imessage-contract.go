/*
 * SPDX-License-Identifier: Apache-2.0
 */

package main

import (
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"strconv"

	"github.com/hyperledger/fabric-contract-api-go/contractapi"
)

// ImessageContract contract for managing CRUD for Imessage
type ImessageContract struct {
	contractapi.Contract
}

// ImessageExists returns true when asset with given ID exists in world state
func (c *ImessageContract) ImessageExists(ctx contractapi.TransactionContextInterface, imessageID string) (bool, error) {
	data, err := ctx.GetStub().GetState(imessageID)

	if err != nil {
		return false, err
	}

	return data != nil, nil
}

// CreateImessage creates a new instance of Imessage
func (c *ImessageContract) CreateImessage(ctx contractapi.TransactionContextInterface, imessageID string, value string) error {
	exists, err := c.ImessageExists(ctx, imessageID)
	if err != nil {
		return fmt.Errorf("Could not read from world state. %s", err)
	} else if exists {
		return fmt.Errorf("The asset %s already exists", imessageID)
	}

	message := new(Message)
	message.Counter = 1
	message.Data = "kiwi"
	message.Signature = "1"

	bytes, _ := json.Marshal(message)

	return ctx.GetStub().PutState(imessageID, bytes)
}

func GetSha256(str string) string {
	srcByte := []byte(str)
	sha256New := sha256.New()
	sha256Bytes := sha256New.Sum(srcByte)
	sha256String := hex.EncodeToString(sha256Bytes)
	return sha256String
}

// ReadImessage retrieves an instance of Imessage from the world state
func (c *ImessageContract) Sign(ctx contractapi.TransactionContextInterface, data string, counter string) (*MessageResponse, error) {
	msg := new(Message)
	icounter, err := strconv.ParseInt(counter, 10, 32)
	if err != nil {
		return nil, fmt.Errorf("Parse Int error. %s", err)
	}
	msg.Counter = int32(icounter)
	msg.Data = data
	msg.Signature = ""
	bytes, _ := json.Marshal(msg)
	msgjson := string(bytes)
	signature := GetSha256(msgjson)
	response := new(MessageResponse)
	response.Signature = signature
	response.Counter = msg.Counter + 1
	return response, err
}

func Verify(msg *Message) bool {
	signature := msg.Signature
	msg.Signature = ""
	msg.Counter = msg.Counter - 1
	bytes, _ := json.Marshal(msg)
	srcMsgjson := string(bytes)
	srcsignature := GetSha256(srcMsgjson)
	boolResult := signature == srcsignature
	return boolResult
}
func (c *ImessageContract) Upload(ctx contractapi.TransactionContextInterface, messageId string, data string, signature string, counter string) bool {

	counter64, _ := strconv.ParseInt(counter, 10, 32)
	counter32 := int32(counter64)

	msg := new(Message)
	msg.Data = data
	msg.Counter = counter32
	msg.Signature = signature

	verify := Verify(msg)
	if verify {
		bytes, _ := json.Marshal(msg)
		ctx.GetStub().PutState(messageId, bytes)
		return true
	}
	return false
}
