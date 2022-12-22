/*
 * SPDX-License-Identifier: Apache-2.0
 */

package main

import (
	"github.com/hyperledger/fabric-contract-api-go/contractapi"
	"github.com/hyperledger/fabric-contract-api-go/metadata"
)

func main() {
	imessageContract := new(ImessageContract)
	imessageContract.Info.Version = "0.0.1"
	imessageContract.Info.Description = "My Smart Contract"
	imessageContract.Info.License = new(metadata.LicenseMetadata)
	imessageContract.Info.License.Name = "Apache-2.0"
	imessageContract.Info.Contact = new(metadata.ContactMetadata)
	imessageContract.Info.Contact.Name = "John Doe"

	chaincode, err := contractapi.NewChaincode(imessageContract)
	chaincode.Info.Title = "imessage chaincode"
	chaincode.Info.Version = "0.0.1"

	if err != nil {
		panic("Could not create chaincode from ImessageContract." + err.Error())
	}

	err = chaincode.Start()

	if err != nil {
		panic("Failed to start chaincode. " + err.Error())
	}
}
